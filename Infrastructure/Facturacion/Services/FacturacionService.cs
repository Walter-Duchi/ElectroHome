using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Infrastructure.Data;
using Infrastructure.Facturacion.Helpers;
using Infrastructure.Facturacion.Models;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SriRecepcion;
using SriAutorizacion;
using PagoModel = Infrastructure.Models.Pago;
using PagoFactura = Infrastructure.Facturacion.Models.Pago;

namespace Infrastructure.Facturacion.Services
{
    public interface IFacturacionService
    {
        Task FacturarVenta(int ventaId);
    }

    public class FacturacionService : IFacturacionService
    {
        private readonly ReclamosContext _context;
        private readonly IFirmaElectronicaService _firmaService;
        private readonly ISriFacturacionService _sriService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FacturacionService> _logger;

        public FacturacionService(
            ReclamosContext context,
            IFirmaElectronicaService firmaService,
            ISriFacturacionService sriService,
            IConfiguration configuration,
            ILogger<FacturacionService> logger)
        {
            _context = context;
            _firmaService = firmaService;
            _sriService = sriService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task FacturarVenta(int ventaId)
        {
            // 1. Obtener datos de la venta, cliente y productos
            var venta = await _context.Ventas
                .Include(v => v.FkEmpresaClienteNavigation)
                .Include(v => v.VentasPorNumeroSerieProductos)
                    .ThenInclude(vp => vp.FkNumeroSerieProductoNavigation)
                        .ThenInclude(nsp => nsp.FkProductoNavigation)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
                throw new Exception("Venta no encontrada");

            // 2. Generar XML de factura
            var factura = new Factura
            {
                InfoTributaria = new InfoTributaria
                {
                    ambiente = "1", // pruebas
                    tipoEmision = "1",
                    razonSocial = "DUCHI RIVERA WALTER ALEJANDRO",
                    nombreComercial = "SOFTWARE HOME",
                    ruc = "0950734061001",
                    claveAcceso = "", // se llenará después
                    codDoc = "01",
                    estab = "001",
                    ptoEmi = "001",
                    secuencial = venta.CodigoFactura.Split('-').Last(),
                    dirMatriz = "RIO AGUARICO Y RIO PASTAZA 123 Y CALLE B, MILAGRO",
                },
                InfoFactura = new InfoFactura
                {
                    fechaEmision = venta.FechaCompra?.ToString("dd/MM/yyyy") ?? "",
                    dirEstablecimiento = "RIO AGUARICO Y RIO PASTAZA 123 Y CALLE B, MILAGRO",
                    obligadoContabilidad = "NO",
                    tipoIdentificacionComprador = venta.FkEmpresaClienteNavigation.TipoIdentificacion switch
                    {
                        "Cedula" => "05",
                        "RUC" => "04",
                        _ => "07"
                    },
                    razonSocialComprador = venta.FkEmpresaClienteNavigation.RazonSocial
                        ?? $"{venta.FkEmpresaClienteNavigation.Nombres} {venta.FkEmpresaClienteNavigation.Apellidos}",
                    identificacionComprador = venta.FkEmpresaClienteNavigation.Identificacion ?? "",
                    direccionComprador = venta.FkEmpresaClienteNavigation.Direccion ?? "",
                    totalSinImpuestos = venta.VentasPorNumeroSerieProductos.Sum(vp => vp.PrecioVenta),
                    totalDescuento = venta.VentasPorNumeroSerieProductos.Sum(vp => vp.Descuento ?? 0),
                    totalConImpuestos = new System.Collections.Generic.List<TotalImpuesto>
                    {
                        new TotalImpuesto
                        {
                            codigo = "2",
                            codigoPorcentaje = "2", // IVA 12%
                            baseImponible = venta.TotalCompra / 1.15m,
                            valor = venta.TotalCompra - (venta.TotalCompra / 1.15m)
                        }
                    },
                    propina = 0,
                    importeTotal = venta.TotalCompra,
                    moneda = "DOLAR",
                    pagos = new System.Collections.Generic.List<PagoFactura>
                    {
                        new PagoFactura
                        {
                            formaPago = "01", // efectivo
                            total = venta.TotalCompra
                        }
                    }
                },
                Detalles = venta.VentasPorNumeroSerieProductos.Select(vp => new Detalle
                {
                    codigoPrincipal = vp.FkNumeroSerieProductoNavigation.FkProductoNavigation.Sku ?? "",
                    descripcion = vp.FkNumeroSerieProductoNavigation.FkProductoNavigation.Descripcion ?? "",
                    cantidad = 1,
                    precioUnitario = vp.PrecioVenta,
                    descuento = vp.Descuento ?? 0,
                    precioTotalSinImpuesto = vp.PrecioVenta,
                    impuestos = new System.Collections.Generic.List<DetalleImpuesto>
                    {
                        new DetalleImpuesto
                        {
                            codigo = "2",
                            codigoPorcentaje = "2",
                            tarifa = 12,
                            baseImponible = vp.PrecioVenta,
                            valor = vp.PrecioVenta * 0.12m
                        }
                    }
                }).ToList(),
                InfoAdicional = new InfoAdicional
                {
                    Campos = new System.Collections.Generic.List<CampoAdicional>
                    {
                        new CampoAdicional { Nombre = "Email", Valor = venta.FkEmpresaClienteNavigation.Correo ?? "" },
                        new CampoAdicional { Nombre = "Teléfono", Valor = venta.FkEmpresaClienteNavigation.Celular ?? "" }
                    }
                }
            };

            // Generar clave de acceso
            var secuencial = venta.CodigoFactura.Split('-').Last().PadLeft(9, '0');
            factura.InfoTributaria.claveAcceso = ClaveAccesoHelper.GenerarClaveAcceso(
                venta.FechaCompra ?? throw new InvalidOperationException("Fecha de compra no puede ser nula"),
                factura.InfoTributaria.ruc,
                factura.InfoTributaria.estab,
                factura.InfoTributaria.ptoEmi,
                secuencial
            );

            // Serializar a XML
            var xmlSinFirma = SerializarAFacturaXml(factura);

            // 3. Firmar electrónicamente
            var xmlFirmado = await _firmaService.FirmarXmlAsync(xmlSinFirma);

            // 4. Enviar al SRI
            var bytesXml = Encoding.UTF8.GetBytes(xmlFirmado);
            SriRecepcion.validarComprobanteResponse respuestaRecepcion;

            try
            {
                respuestaRecepcion = await _sriService.EnviarComprobante(bytesXml);
                _logger.LogInformation("Respuesta de recepción: {Estado}", respuestaRecepcion.RespuestaRecepcionComprobante.estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar comprobante al SRI");
                venta.EstadoSri = "ErrorEnvio";
                await _context.SaveChangesAsync();
                throw; // Opcional: relanzar para que el endpoint sepa que falló
            }

            if (respuestaRecepcion.RespuestaRecepcionComprobante.estado == "RECIBIDA")
            {
                try
                {
                    // Esperar y consultar autorización
                    await Task.Delay(3000);
                    var respuestaAutorizacion = await _sriService.ConsultarAutorizacion(factura.InfoTributaria.claveAcceso);

                    if (respuestaAutorizacion.RespuestaAutorizacionComprobante.autorizaciones?.Length > 0)
                    {
                        var autorizacion = respuestaAutorizacion.RespuestaAutorizacionComprobante.autorizaciones[0];
                        venta.EstadoSri = autorizacion.estado == "AUTORIZADO" ? "Autorizado" : "Rechazado";
                        venta.FechaAutorizacion = DateTime.Now;

                        // Guardar XML autorizado
                        var rutaXml = Path.Combine(_configuration["RutaXmlAutorizados"] ?? "XmlAutorizados",
                                                   $"{factura.InfoTributaria.claveAcceso}.xml");
                        Directory.CreateDirectory(Path.GetDirectoryName(rutaXml)!);
                        await File.WriteAllTextAsync(rutaXml, xmlFirmado);
                        venta.XmlPath = rutaXml;
                    }
                    else
                    {
                        venta.EstadoSri = "ErrorSinAutorizacion";
                        _logger.LogWarning("No se recibieron autorizaciones para la clave {Clave}", factura.InfoTributaria.claveAcceso);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al consultar autorización para la clave {Clave}", factura.InfoTributaria.claveAcceso);
                    venta.EstadoSri = "ErrorConsulta";
                }
            }
            else
            {
                venta.EstadoSri = "Rechazado";
                if (respuestaRecepcion.RespuestaRecepcionComprobante.comprobantes?.Length > 0)
                {
                    var errores = respuestaRecepcion.RespuestaRecepcionComprobante.comprobantes[0].mensajes;
                    _logger.LogError("Errores en recepción: {@errores}", errores);
                }
            }

            await _context.SaveChangesAsync();
        }

        private string SerializarAFacturaXml(Factura factura)
        {
            var serializer = new XmlSerializer(typeof(Factura));
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, factura, ns);
            return stringWriter.ToString();
        }
    }
}