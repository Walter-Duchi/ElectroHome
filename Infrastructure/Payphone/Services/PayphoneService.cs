using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Data;
using Infrastructure.Facturacion.Services;
using Infrastructure.Models;
using Infrastructure.Payphone.DTOs;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Payphone.Services
{
    public class PayphoneService : IPayphoneService
    {
        private readonly ReclamosContext _context;
        private readonly ICartService _cartService;
        private readonly IFacturacionService _facturacionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayphoneService> _logger;
        private readonly HttpClient _httpClient;

        private readonly string _payphoneApiBase;
        private readonly string _payphoneToken;
        private readonly string _payphoneStoreId;

        public PayphoneService(
            ReclamosContext context,
            ICartService cartService,
            IFacturacionService facturacionService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<PayphoneService> logger)
        {
            _context = context;
            _cartService = cartService;
            _facturacionService = facturacionService;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();

            _payphoneApiBase = _configuration["Payphone:ApiBase"] ?? "https://pay.payphonetodoesposible.com";
            _payphoneToken = _configuration["Payphone:Token"]!;
            _payphoneStoreId = _configuration["Payphone:StoreId"]!;
        }

        public async Task<PayphoneInitResponse> InitializeTransactionAsync(int usuarioId)
        {
            // 1. Obtener el carrito del usuario
            var cartItems = await _cartService.GetCartAsync(usuarioId);
            if (!cartItems.Any())
                throw new Exception("El carrito está vacío");

            // 2. Calcular totales usando la misma lógica que en facturación
            decimal totalSinImpuestos = 0m;
            decimal totalIva = 0m;
            decimal totalConImpuestos = 0m;

            foreach (var item in cartItems)
            {
                // Calcular IVA como en factura: PrecioUnitario * 15 / 115
                var iva = Math.Round(item.PrecioUnitario * 15 / 115, 2);
                var baseImponible = item.PrecioUnitario - iva;
                totalSinImpuestos += baseImponible * item.Cantidad;
                totalIva += iva * item.Cantidad;
                totalConImpuestos += item.PrecioUnitario * item.Cantidad;
            }

            // Total en centavos (redondeo a entero)
            int totalAmount = (int)Math.Round(totalConImpuestos * 100, MidpointRounding.AwayFromZero);
            int totalWithoutTax = 0; // productos exentos
            int totalWithTax = (int)Math.Round(totalSinImpuestos * 100, MidpointRounding.AwayFromZero);
            int totalTax = (int)Math.Round(totalIva * 100, MidpointRounding.AwayFromZero);

            // 3. Generar clientTransactionId único (máx 15 caracteres)
            string clientTxId = Guid.NewGuid().ToString("N").Substring(0, 15).ToUpper();

            // 4. Guardar transacción pendiente en BD
            var pendingTx = new PayphoneTransaction
            {
                ClientTransactionId = clientTxId,
                FkUsuario = usuarioId,
                MontoTotal = totalConImpuestos,
                DatosCarrito = JsonSerializer.Serialize(cartItems),
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now
            };
            _context.Set<PayphoneTransaction>().Add(pendingTx);
            await _context.SaveChangesAsync();

            // 5. Devolver datos para la cajita
            return new PayphoneInitResponse
            {
                ClientTransactionId = clientTxId,
                Amount = totalAmount,
                AmountWithoutTax = totalWithoutTax,
                AmountWithTax = totalWithTax,
                Tax = totalTax,
                Token = _payphoneToken,
                StoreId = _payphoneStoreId,
                Reference = $"Compra en ElectroHome - {DateTime.Now:yyyyMMddHHmmss}",
                Currency = "USD"
            };
        }

        public async Task<PayphoneConfirmResponse> ConfirmTransactionAsync(PayphoneConfirmRequest request, int usuarioId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Recuperar la transacción pendiente con bloqueo para actualización (FOR UPDATE)
                var pending = await _context.Set<PayphoneTransaction>()
                    .FromSqlInterpolated($@"
                        SELECT * FROM PayphoneTransactions WITH (UPDLOCK, ROWLOCK)
                        WHERE ClientTransactionId = {request.ClientTransactionId} AND FkUsuario = {usuarioId}")
                    .FirstOrDefaultAsync();

                if (pending == null)
                    throw new Exception("Transacción no encontrada o no corresponde al usuario");

                if (pending.Estado != "Pendiente")
                    throw new Exception("La transacción ya fue procesada");

                // 2. Llamar a la API de Payphone para confirmar
                var confirmUrl = $"{_payphoneApiBase}/api/button/V2/Confirm";
                var confirmPayload = new
                {
                    id = request.Id,
                    clientTxId = request.ClientTransactionId
                };

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, confirmUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _payphoneToken);
                httpRequest.Content = new StringContent(JsonSerializer.Serialize(confirmPayload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al confirmar pago con Payphone: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    pending.Estado = "Fallido";
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    throw new Exception("Error al confirmar el pago con Payphone");
                }

                var confirmResult = JsonSerializer.Deserialize<PayphoneApiConfirmResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (confirmResult == null)
                    throw new Exception("Respuesta inválida de Payphone");

                // 3. Verificar estado
                if (confirmResult.StatusCode != 3 || confirmResult.TransactionStatus != "Approved")
                {
                    pending.Estado = "Fallido";
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    throw new Exception($"Pago no aprobado: {confirmResult.Message ?? "Desconocido"}");
                }

                // 4. Pago aprobado: crear venta y facturar
                // Deserializar los items del carrito guardados
                var cartItems = JsonSerializer.Deserialize<System.Collections.Generic.List<Application.DTOs.Ecommerce.CartItemResponse>>(pending.DatosCarrito);
                if (cartItems == null)
                    throw new Exception("No se pudieron recuperar los items del carrito");

                // Verificar stock nuevamente (pudo haber cambiado)
                foreach (var item in cartItems)
                {
                    var stock = await _context.NumeroSerieProductos
                        .CountAsync(n => n.FkProducto == item.ProductoId
                            && n.EstadoInventario == "Se_Puede_Vender"
                            && !_context.VentasPorNumeroSerieProductos.Any(v => v.FkNumeroSerieProducto == n.Id));
                    if (stock < item.Cantidad)
                        throw new Exception($"Stock insuficiente para el producto {item.NombreProducto}");
                }

                // Crear venta
                var venta = new Venta
                {
                    CodigoFactura = $"E-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}",
                    FkEmpresaCliente = usuarioId,
                    FkVendedor = null,
                    TipoVenta = "Contado",
                    FechaCompra = DateTime.Now,
                    EstadoSri = "Pendiente",
                    TotalCompra = pending.MontoTotal,
                    Observaciones = "Venta generada desde pago Payphone",
                    DireccionEntrega = "Por definir",
                    TelefonoContacto = "",
                    CreadoPor = usuarioId
                };
                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync(); // para obtener el Id

                // Asignar números de serie a los productos
                foreach (var item in cartItems)
                {
                    var seriesDisponibles = await _context.NumeroSerieProductos
                        .Where(n => n.FkProducto == item.ProductoId
                            && n.EstadoInventario == "Se_Puede_Vender"
                            && !_context.VentasPorNumeroSerieProductos.Any(v => v.FkNumeroSerieProducto == n.Id))
                        .OrderBy(n => n.Id)
                        .Take(item.Cantidad)
                        .ToListAsync();

                    if (seriesDisponibles.Count < item.Cantidad)
                        throw new Exception($"No hay suficientes unidades disponibles para {item.NombreProducto}");

                    foreach (var serie in seriesDisponibles)
                    {
                        serie.EstadoInventario = "Vendido";
                        var ventaProducto = new VentasPorNumeroSerieProducto
                        {
                            FkVentas = venta.Id,
                            FkNumeroSerieProducto = serie.Id,
                            PrecioVenta = item.PrecioUnitario,
                            Descuento = 0,
                            Iva = Math.Round(item.PrecioUnitario * 15 / 115, 2)
                        };
                        _context.VentasPorNumeroSerieProductos.Add(ventaProducto);
                    }
                }

                // Registrar pago
                var metodoPayphone = await _context.MetodosPagos.FirstOrDefaultAsync(m => m.Tipo == "Payphone");
                if (metodoPayphone == null)
                    throw new Exception("Método de pago Payphone no configurado");

                var pago = new Pago
                {
                    FkVenta = venta.Id,
                    FkMetodoPago = metodoPayphone.Id,
                    Estado = "Completo",
                    Monto = pending.MontoTotal,
                    Referencia = confirmResult.AuthorizationCode,
                    FechaPago = DateTime.Now,
                    DatosTransaccion = JsonSerializer.Serialize(confirmResult)
                };
                _context.Pagos.Add(pago);

                // Actualizar transacción pendiente
                pending.Estado = "Completado";
                pending.PayphoneId = confirmResult.TransactionId;
                pending.VentaId = venta.Id;

                await _context.SaveChangesAsync();

                // Ahora facturar electrónicamente (dentro de la transacción)
                try
                {
                    await _facturacionService.FacturarVenta(venta.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al facturar la venta {VentaId}", venta.Id);
                    await transaction.RollbackAsync();
                    throw new Exception("Error al generar la factura electrónica. El pago fue procesado pero no se pudo facturar. Contacte al administrador.", ex);
                }

                // Confirmar transacción de base de datos
                await transaction.CommitAsync();

                // Una vez que todo es exitoso, vaciar el carrito del usuario
                await _cartService.ClearCartAsync(usuarioId);

                // Recargar venta para obtener datos actualizados (clave de acceso, etc.)
                venta = await _context.Ventas.FindAsync(venta.Id);

                return new PayphoneConfirmResponse
                {
                    TransactionId = confirmResult.TransactionId,
                    ClientTransactionId = confirmResult.ClientTransactionId,
                    Status = confirmResult.TransactionStatus,
                    StatusCode = confirmResult.StatusCode,
                    Amount = confirmResult.Amount / 100m,
                    AuthorizationCode = confirmResult.AuthorizationCode,
                    Message = confirmResult.Message,
                    VentaId = venta.Id,
                    PdfUrl = $"/api/factura/pdf/{venta.Id}"
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Clase auxiliar para mapear respuesta de Payphone
        private class PayphoneApiConfirmResponse
        {
            public long TransactionId { get; set; }
            public string ClientTransactionId { get; set; } = string.Empty;
            public int StatusCode { get; set; }
            public string TransactionStatus { get; set; } = string.Empty;
            public int Amount { get; set; }
            public string AuthorizationCode { get; set; } = string.Empty;
            public string? Message { get; set; }
        }
    }
}