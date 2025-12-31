using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using Application.DTOs.Reclamo;
using Infrastructure.Models;
using System.Transactions;

namespace Infrastructure.Services
{
    public class ReclamoService : IReclamoService
    {
        private readonly ReclamosContext _context;
        private readonly ILogger<ReclamoService> _logger;

        public ReclamoService(ReclamosContext context, ILogger<ReclamoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ValidarClienteResponse> ValidarClienteAsync(string ruc)
        {
            try
            {
                var cliente = await _context.Usuarios
                    .Where(u => u.Ruc == ruc && u.Rol == "Cliente")
                    .Select(u => new { u.Id, u.Nombres, u.Apellidos, u.Ruc })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return new ValidarClienteResponse
                    {
                        EsValido = false,
                        Mensaje = "Cliente no encontrado o no tiene rol de cliente."
                    };
                }

                return new ValidarClienteResponse
                {
                    EsValido = true,
                    Mensaje = "Cliente válido.",
                    ClienteId = cliente.Id,
                    RazonSocial = $"{cliente.Nombres} {cliente.Apellidos}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar cliente con RUC: {Ruc}", ruc);
                return new ValidarClienteResponse
                {
                    EsValido = false,
                    Mensaje = "Error interno al validar cliente."
                };
            }
        }

        public async Task<ValidarProductoResponse> ValidarProductoAsync(string numeroSerie)
        {
            try
            {
                var producto = await _context.NumeroSerieProductos
                    .Include(nsp => nsp.FkProductoNavigation)
                        .ThenInclude(p => p.FkMarcaNavigation)
                    .Include(nsp => nsp.VentasPorNumeroSerieProducto)
                        .ThenInclude(vpsp => vpsp.FkVentasNavigation)
                    .FirstOrDefaultAsync(nsp => nsp.NumeroSerie == numeroSerie);

                if (producto == null)
                {
                    return new ValidarProductoResponse
                    {
                        EsValido = false,
                        Mensaje = "Producto no encontrado."
                    };
                }

                if (producto.EstadoInventario != "Vendido" && producto.EstadoInventario != "Entregado_Como_Reemplazo_Al_Cliente")
                {
                    return new ValidarProductoResponse
                    {
                        EsValido = false,
                        Mensaje = $"El producto no está disponible para reclamar. Estado: {producto.EstadoInventario}"
                    };
                }

                DateTime? fechaVenta = null;
                if (producto.VentasPorNumeroSerieProducto != null)
                {
                    fechaVenta = producto.VentasPorNumeroSerieProducto.FkVentasNavigation?.FechaCompra;
                }

                bool tieneGarantia = false;
                int? diasGarantia = producto.FkProductoNavigation?.DiasGarantia;

                if (diasGarantia == 0)
                {
                    return new ValidarProductoResponse
                    {
                        EsValido = false,
                        Mensaje = "Producto descontinuado, no tiene garantía.",
                        ProductoId = producto.Id,
                        Marca = producto.FkProductoNavigation?.FkMarcaNavigation?.Nombre,
                        Modelo = producto.FkProductoNavigation?.Modelo,
                        TieneGarantia = false,
                        EstadoInventario = producto.EstadoInventario,
                        FechaVenta = fechaVenta,
                        DiasGarantia = diasGarantia,
                        Especificacion = producto.FkProductoNavigation?.Especificacion,
                        Precio = producto.FkProductoNavigation?.Precio
                    };
                }

                if (fechaVenta.HasValue && diasGarantia.HasValue)
                {
                    var fechaLimiteGarantia = fechaVenta.Value.AddDays(diasGarantia.Value);
                    if (fechaLimiteGarantia >= DateTime.Now)
                    {
                        tieneGarantia = true;
                    }
                }

                if (!tieneGarantia)
                {
                    return new ValidarProductoResponse
                    {
                        EsValido = false,
                        Mensaje = "El producto ha excedido el período de garantía.",
                        ProductoId = producto.Id,
                        Marca = producto.FkProductoNavigation?.FkMarcaNavigation?.Nombre,
                        Modelo = producto.FkProductoNavigation?.Modelo,
                        TieneGarantia = false,
                        EstadoInventario = producto.EstadoInventario,
                        FechaVenta = fechaVenta,
                        DiasGarantia = diasGarantia,
                        Especificacion = producto.FkProductoNavigation?.Especificacion,
                        Precio = producto.FkProductoNavigation?.Precio
                    };
                }

                return new ValidarProductoResponse
                {
                    EsValido = true,
                    Mensaje = "Producto válido para reclamo.",
                    ProductoId = producto.Id,
                    Marca = producto.FkProductoNavigation?.FkMarcaNavigation?.Nombre,
                    Modelo = producto.FkProductoNavigation?.Modelo,
                    TieneGarantia = true,
                    EstadoInventario = producto.EstadoInventario,
                    FechaVenta = fechaVenta,
                    DiasGarantia = diasGarantia,
                    Especificacion = producto.FkProductoNavigation?.Especificacion,
                    Precio = producto.FkProductoNavigation?.Precio
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar producto con número de serie: {NumeroSerie}", numeroSerie);
                return new ValidarProductoResponse
                {
                    EsValido = false,
                    Mensaje = "Error interno al validar producto."
                };
            }
        }

        public async Task<CrearReclamoResponse> CrearReclamoAsync(CrearReclamoRequest request, int revisorId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("========================================");
                _logger.LogInformation("INICIANDO CREACIÓN DE RECLAMO");
                _logger.LogInformation("RevisorId: {RevisorId}", revisorId);
                _logger.LogInformation("RUC Cliente: {RucCliente}", request.RucCliente);
                _logger.LogInformation("Productos solicitados: {@Productos}", request.Productos);
                _logger.LogInformation("========================================");

                // 1. Validar cliente
                _logger.LogInformation("Validando cliente con RUC: {RucCliente}", request.RucCliente);
                var cliente = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Ruc == request.RucCliente && u.Rol == "Cliente");

                if (cliente == null)
                {
                    _logger.LogError("Cliente no encontrado con RUC: {RucCliente}", request.RucCliente);
                    return new CrearReclamoResponse
                    {
                        Exito = false,
                        Mensaje = "Cliente no válido."
                    };
                }
                _logger.LogInformation("Cliente validado: {ClienteId} - {Nombre}", cliente.Id, $"{cliente.Nombres} {cliente.Apellidos}");

                // 2. Validar cada producto
                var productosReclamados = new List<(int numeroSerieProductoId, string formaCompensacion, string numeroSerie)>();

                foreach (var productoReq in request.Productos)
                {
                    _logger.LogInformation("Validando producto: {NumeroSerie}", productoReq.NumeroSerie);

                    // Primero, verificar si el producto ya está en un reclamo
                    var productoExistente = await _context.NumeroSerieProductos
                        .FirstOrDefaultAsync(nsp => nsp.NumeroSerie == productoReq.NumeroSerie);

                    if (productoExistente == null)
                    {
                        _logger.LogError("Producto no encontrado: {NumeroSerie}", productoReq.NumeroSerie);
                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = $"Producto {productoReq.NumeroSerie} no encontrado."
                        };
                    }

                    // Verificar si ya está en un reclamo activo
                    var yaEnReclamo = await _context.ReclamosProductoSns
                        .AnyAsync(rps => rps.FkNumeroSerieProductos == productoExistente.Id &&
                                       rps.Estado != "Compensado");

                    if (yaEnReclamo)
                    {
                        _logger.LogError("Producto {NumeroSerie} ya está en un reclamo activo", productoReq.NumeroSerie);
                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = $"El producto {productoReq.NumeroSerie} ya está en un reclamo activo y no puede ser reclamado nuevamente."
                        };
                    }

                    var validacion = await ValidarProductoAsync(productoReq.NumeroSerie);

                    _logger.LogInformation("Resultado validación producto {NumeroSerie}: EsValido={EsValido}, TieneGarantia={TieneGarantia}, Mensaje={Mensaje}",
                        productoReq.NumeroSerie, validacion.EsValido, validacion.TieneGarantia, validacion.Mensaje);

                    if (!validacion.EsValido || !validacion.TieneGarantia)
                    {
                        _logger.LogError("Producto no válido: {NumeroSerie} - {Mensaje}",
                            productoReq.NumeroSerie, validacion.Mensaje);

                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = $"Producto {productoReq.NumeroSerie} no válido: {validacion.Mensaje}"
                        };
                    }

                    if (!validacion.ProductoId.HasValue)
                    {
                        _logger.LogError("ProductoId es null para: {NumeroSerie}", productoReq.NumeroSerie);
                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = $"Error interno: ProductoId no encontrado para {productoReq.NumeroSerie}"
                        };
                    }

                    productosReclamados.Add((validacion.ProductoId.Value, productoReq.FormaCompensacion, productoReq.NumeroSerie));
                    _logger.LogInformation("Producto {NumeroSerie} agregado con ID: {ProductoId}",
                        productoReq.NumeroSerie, validacion.ProductoId.Value);
                }

                // 3. Crear código de reclamo
                var codigoReclamo = $"REC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                _logger.LogInformation("Código de reclamo generado: {CodigoReclamo}", codigoReclamo);

                // 4. Crear entidad Reclamo
                var reclamo = new Reclamo
                {
                    CodigoReclamo = codigoReclamo,
                    FkEmpresaCliente = cliente.Id,
                    FechaCreacionReclamo = DateTime.Now
                };

                _context.Reclamos.Add(reclamo);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Reclamo creado en BD con ID: {ReclamoId}", reclamo.Id);

                // 5. Crear productos del reclamo
                var reclamosProductos = new List<ReclamosProductoSn>();

                foreach (var (numeroSerieProductoId, formaCompensacion, numeroSerie) in productosReclamados)
                {
                    _logger.LogInformation("Asignando técnico para producto ID: {ProductoId}", numeroSerieProductoId);

                    var tecnicoAsignado = await AsignarTecnicoAsync(numeroSerieProductoId);

                    if (tecnicoAsignado == null)
                    {
                        _logger.LogError("No se pudo asignar técnico para producto ID: {ProductoId}", numeroSerieProductoId);
                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = "No se pudo asignar un técnico para uno de los productos."
                        };
                    }

                    _logger.LogInformation("Técnico asignado: {TecnicoId} - {Nombre}",
                        tecnicoAsignado.Id, $"{tecnicoAsignado.Nombres} {tecnicoAsignado.Apellidos}");

                    var reclamoProducto = new ReclamosProductoSn
                    {
                        FkNumeroSerieProductos = numeroSerieProductoId,
                        FkReclamos = reclamo.Id,
                        FormaCompensacion = formaCompensacion,
                        Estado = "Pendiente",
                        FkTecnicoAsignado = tecnicoAsignado.Id,
                        FechaReclamoClienteFinal = DateTime.Now
                    };

                    reclamosProductos.Add(reclamoProducto);
                    _context.ReclamosProductoSns.Add(reclamoProducto);

                    _logger.LogInformation("Producto {NumeroSerie} agregado al reclamo con técnico {TecnicoId}",
                        numeroSerie, tecnicoAsignado.Id);
                }

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Todos los productos agregados al reclamo");
                }
                catch (DbUpdateException dbEx)
                {
                    // Capturar la excepción interna específica de SQL
                    var innerException = dbEx.InnerException;
                    while (innerException != null)
                    {
                        _logger.LogError("Inner Exception: {Message}", innerException.Message);
                        innerException = innerException.InnerException;
                    }

                    await transaction.RollbackAsync();
                    return new CrearReclamoResponse
                    {
                        Exito = false,
                        Mensaje = $"Error de base de datos al guardar el reclamo: {dbEx.InnerException?.Message ?? dbEx.Message}"
                    };
                }

                // 6. Generar PDF
                _logger.LogInformation("Generando PDF para reclamo {ReclamoId}", reclamo.Id);
                var pdfBase64 = await GenerarPdfAsync(reclamo.Id, codigoReclamo, cliente, productosReclamados);

                if (string.IsNullOrEmpty(pdfBase64))
                {
                    _logger.LogWarning("PDF generado vacío para reclamo {ReclamoId}", reclamo.Id);
                }
                else
                {
                    _logger.LogInformation("PDF generado correctamente para reclamo {ReclamoId}", reclamo.Id);
                }

                // 7. Commit de transacción
                await transaction.CommitAsync();
                _logger.LogInformation("Transacción completada exitosamente");

                return new CrearReclamoResponse
                {
                    Exito = true,
                    Mensaje = "Reclamo creado exitosamente.",
                    ReclamoId = reclamo.Id,
                    CodigoReclamo = codigoReclamo,
                    PdfBase64 = pdfBase64,
                    PdfFileName = $"{codigoReclamo}.pdf"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear reclamo para el cliente con RUC: {Ruc}", request.RucCliente);

                // Capturar y mostrar la excepción interna completa
                var errorMessage = ex.Message;
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    errorMessage += $" -> {innerEx.Message}";
                    innerEx = innerEx.InnerException;
                }

                return new CrearReclamoResponse
                {
                    Exito = false,
                    Mensaje = $"Error interno al crear el reclamo: {errorMessage}"
                };
            }
        }


        private async Task<Usuario?> AsignarTecnicoAsync(int numeroSerieProductoId)
        {
            var producto = await _context.NumeroSerieProductos
                .Include(nsp => nsp.FkProductoNavigation)
                .FirstOrDefaultAsync(nsp => nsp.Id == numeroSerieProductoId);

            if (producto == null) return null;

            var marcaId = producto.FkProductoNavigation.FkMarca;

            var tecnicosCertificados = await _context.UsuariosCertificacionMarcas
                .Where(ucm => ucm.FkMarca == marcaId)
                .Select(ucm => ucm.FkTecnicoNavigation)
                .Where(t => t.Rol == "Tecnico")
                .ToListAsync();

            if (!tecnicosCertificados.Any()) return null;

            var cargaTrabajo = await _context.ReclamosProductoSns
                .Where(rps => rps.FkTecnicoAsignado != null && rps.Estado == "Pendiente")
                .GroupBy(rps => rps.FkTecnicoAsignado)
                .Select(g => new { TecnicoId = g.Key, Carga = g.Count() })
                .ToDictionaryAsync(x => x.TecnicoId, x => x.Carga);

            Usuario? tecnicoAsignado = null;
            int minCarga = int.MaxValue;

            foreach (var tecnico in tecnicosCertificados)
            {
                cargaTrabajo.TryGetValue(tecnico.Id, out int carga);
                if (carga < minCarga)
                {
                    minCarga = carga;
                    tecnicoAsignado = tecnico;
                }
            }

            return tecnicoAsignado;
        }

        private async Task<string> GenerarPdfAsync(int reclamoId, string codigoReclamo, Usuario cliente, List<(int numeroSerieProductoId, string formaCompensacion, string numeroSerie)> productos)
        {
            try
            {
                var productosDetalle = new List<dynamic>();
                foreach (var (id, forma, numeroSerie) in productos)
                {
                    var producto = await _context.NumeroSerieProductos
                        .Include(nsp => nsp.FkProductoNavigation)
                            .ThenInclude(p => p.FkMarcaNavigation)
                        .FirstOrDefaultAsync(nsp => nsp.Id == id);

                    if (producto != null)
                    {
                        productosDetalle.Add(new
                        {
                            NumeroSerie = numeroSerie,
                            Marca = producto.FkProductoNavigation?.FkMarcaNavigation?.Nombre,
                            Modelo = producto.FkProductoNavigation?.Modelo,
                            Especificacion = producto.FkProductoNavigation?.Especificacion,
                            FormaCompensacion = forma,
                            Precio = producto.FkProductoNavigation?.Precio
                        });
                    }
                }

                var pdfContent = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 40px; }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .title {{ font-size: 24px; font-weight: bold; color: #333; }}
                        .subtitle {{ font-size: 18px; color: #666; margin-top: 10px; }}
                        .section {{ margin: 20px 0; }}
                        .section-title {{ font-size: 16px; font-weight: bold; color: #333; border-bottom: 2px solid #0056b3; padding-bottom: 5px; }}
                        .info {{ margin: 10px 0; }}
                        .info-label {{ font-weight: bold; color: #555; }}
                        table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
                        th {{ background-color: #f8f9fa; text-align: left; padding: 10px; border: 1px solid #dee2e6; }}
                        td {{ padding: 10px; border: 1px solid #dee2e6; }}
                        .footer {{ margin-top: 40px; text-align: center; color: #6c757d; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <div class='title'>COMPROBANTE DE RECLAMO</div>
                        <div class='subtitle'>Código: {codigoReclamo}</div>
                    </div>

                    <div class='section'>
                        <div class='section-title'>Información del Cliente</div>
                        <div class='info'><span class='info-label'>Razón Social:</span> {cliente.Nombres} {cliente.Apellidos}</div>
                        <div class='info'><span class='info-label'>RUC:</span> {cliente.Ruc}</div>
                        <div class='info'><span class='info-label'>Fecha de Reclamo:</span> {DateTime.Now:dd/MM/yyyy HH:mm}</div>
                    </div>

                    <div class='section'>
                        <div class='section-title'>Productos Reclamados</div>
                        <table>
                            <thead>
                                <tr>
                                    <th>N° Serie</th>
                                    <th>Marca</th>
                                    <th>Modelo</th>
                                    <th>Especificación</th>
                                    <th>Forma de Compensación</th>
                                    <th>Precio</th>
                                </tr>
                            </thead>
                            <tbody>
                                {string.Join("", productosDetalle.Select(p => $@"
                                <tr>
                                    <td>{p.NumeroSerie}</td>
                                    <td>{p.Marca}</td>
                                    <td>{p.Modelo}</td>
                                    <td>{p.Especificacion}</td>
                                    <td>{p.FormaCompensacion}</td>
                                    <td>${p.Precio:N2}</td>
                                </tr>
                                "))}
                            </tbody>
                        </table>
                    </div>

                    <div class='section'>
                        <div class='section-title'>Observaciones</div>
                        <div class='info'>
                            <p>1. Este comprobante debe ser presentado para cualquier consulta sobre el estado del reclamo.</p>
                            <p>2. Los productos serán revisados por técnicos certificados en las marcas correspondientes.</p>
                            <p>3. El tiempo de resolución dependerá de la complejidad del caso y la disponibilidad de repuestos.</p>
                            <p>4. Para más información, contactar al departamento de soporte técnico.</p>
                        </div>
                    </div>

                    <div class='footer'>
                        <p>Sistema de Gestión de Reclamos - Versión 1.0</p>
                        <p>Documento generado automáticamente el {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                    </div>
                </body>
                </html>";

                // En una implementación real, usaríamos una librería como iTextSharp o QuestPDF
                // Por ahora, devolvemos HTML que puede ser convertido a PDF en el frontend
                return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(pdfContent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF para reclamo {ReclamoId}", reclamoId);
                return string.Empty;
            }
        }
    }
}