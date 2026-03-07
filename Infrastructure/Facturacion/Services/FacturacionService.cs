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

            // Calcular secuencial de 9 dígitos (rellenar con ceros a la izquierda)
            var secuencial = venta.CodigoFactura.Split('-').Last().PadLeft(9, '0');

            // Calcular totales correctamente a partir de los detalles
            var detallesFactura = venta.VentasPorNumeroSerieProductos.Select(vp => new
            {
                BaseImponible = vp.PrecioVenta - vp.Iva, // precio sin IVA
                Iva = vp.Iva,
                Descuento = vp.Descuento ?? 0,
                Sku = vp.FkNumeroSerieProductoNavigation.FkProductoNavigation.Sku ?? "",
                Descripcion = vp.FkNumeroSerieProductoNavigation.FkProductoNavigation.Descripcion ?? "",
                PrecioVenta = vp.PrecioVenta,
                IVA = vp.Iva
            }).ToList();

            var totalSinImpuestos = detallesFactura.Sum(d => d.BaseImponible);
            var totalIva = detallesFactura.Sum(d => d.Iva);
            var totalDescuento = detallesFactura.Sum(d => d.Descuento);

            // Validar que el total de la venta coincida con los cálculos (solo logging)
            if (Math.Abs(totalSinImpuestos + totalIva - venta.TotalCompra) > 0.01m)
            {
                _logger.LogWarning("Inconsistencia en totales: venta.TotalCompra={VentaTotal}, calculado={Calculado}",
                    venta.TotalCompra, totalSinImpuestos + totalIva);
            }

            // 2. Generar XML de factura (versión 1.0.0)
            var factura = new Factura
            {
                InfoTributaria = new InfoTributaria
                {
                    ambiente = "1", // pruebas
                    tipoEmision = "1", // normal
                    razonSocial = "DUCHI RIVERA WALTER ALEJANDRO",
                    nombreComercial = "SOFTWARE HOME",
                    ruc = "0950734061001",
                    claveAcceso = "", // se llenará después
                    codDoc = "01", // factura
                    estab = "001",
                    ptoEmi = "001",
                    secuencial = secuencial,
                    dirMatriz = "RIO AGUARICO Y RIO PASTAZA 123 Y CALLE B, MILAGRO",
                    contribuyenteRimpe = "CONTRIBUYENTE NEGOCIO POPULAR - RÉGIMEN RIMPE",
                },
                InfoFactura = new InfoFactura
                {
                    fechaEmision = venta.FechaCompra?.ToString("dd/MM/yyyy") ?? "",
                    dirEstablecimiento = "RIO AGUARICO Y RIO PASTAZA 123 Y CALLE B, MILAGRO",
                    obligadoContabilidad = "NO",
                    // Para evitar el error de consumidor final con monto > 50 USD, usamos un receptor de prueba
                    tipoIdentificacionComprador = "04", // RUC
                    razonSocialComprador = "PRUEBAS SERVICIO DE RENTAS INTERNAS",
                    identificacionComprador = "1790012347001", // RUC de prueba según ficha técnica
                    direccionComprador = "QUITO",
                    totalSinImpuestos = Math.Round(totalSinImpuestos, 2),
                    totalDescuento = Math.Round(totalDescuento, 2),
                    totalConImpuestos = new System.Collections.Generic.List<TotalImpuesto>
                    {
                        new TotalImpuesto
                        {
                            codigo = "2", // IVA
                            codigoPorcentaje = "2", // 12% (código según tabla 17)
                            baseImponible = Math.Round(totalSinImpuestos, 2),
                            valor = Math.Round(totalIva, 2)
                        }
                    },
                    propina = 0,
                    importeTotal = venta.TotalCompra, // debe ser totalSinImpuestos + totalIva
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
                    CodigoPrincipal = vp.FkNumeroSerieProductoNavigation.FkProductoNavigation.Sku ?? "",
                    CodigoAuxiliar = "", // vacío, no se serializará
                    descripcion = vp.FkNumeroSerieProductoNavigation.FkProductoNavigation.Descripcion ?? "",
                    cantidad = 1,
                    precioUnitario = Math.Round(vp.PrecioVenta - vp.Iva, 2), // precio sin IVA
                    descuento = Math.Round(vp.Descuento ?? 0, 2),
                    precioTotalSinImpuesto = Math.Round(vp.PrecioVenta - vp.Iva, 2),
                    impuestos = new System.Collections.Generic.List<DetalleImpuesto>
                    {
                        new DetalleImpuesto
                        {
                            codigo = "2",
                            codigoPorcentaje = "2",
                            tarifa = 12,
                            baseImponible = Math.Round(vp.PrecioVenta - vp.Iva, 2),
                            valor = Math.Round(vp.Iva, 2)
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

            // Generar clave de acceso (usa el mismo secuencial de 9 dígitos)
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

            // Guardar XML firmado para inspección manual (opcional)
            File.WriteAllText("C:/temp/xmlFirmado.xml", xmlFirmado);

            // 4. Enviar al SRI
            var bytesXml = Encoding.UTF8.GetBytes(xmlFirmado);
            SriRecepcion.validarComprobanteResponse respuestaRecepcion;

            try
            {
                respuestaRecepcion = await _sriService.EnviarComprobante(bytesXml);
                _logger.LogInformation("Respuesta de recepción: {Estado}", respuestaRecepcion?.RespuestaRecepcionComprobante?.estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar comprobante al SRI");
                venta.EstadoSri = "Rechazado"; // Usar estado permitido
                await _context.SaveChangesAsync();
                throw;
            }

            if (respuestaRecepcion?.RespuestaRecepcionComprobante?.estado == "RECIBIDA")
            {
                try
                {
                    // Esperar y consultar autorización
                    await Task.Delay(3000);
                    var respuestaAutorizacion = await _sriService.ConsultarAutorizacion(factura.InfoTributaria.claveAcceso);

                    if (respuestaAutorizacion?.RespuestaAutorizacionComprobante?.autorizaciones?.Length > 0)
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
                        venta.EstadoSri = "Rechazado"; // No se recibieron autorizaciones
                        _logger.LogWarning("No se recibieron autorizaciones para la clave {Clave}", factura.InfoTributaria.claveAcceso);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al consultar autorización para la clave {Clave}", factura.InfoTributaria.claveAcceso);
                    venta.EstadoSri = "Rechazado"; // Error en consulta
                }
            }
            else
            {
                venta.EstadoSri = "Rechazado";
                if (respuestaRecepcion?.RespuestaRecepcionComprobante?.comprobantes?.Length > 0)
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
            var xml = stringWriter.ToString();
            // Reemplazar la declaración de encoding de utf-16 a utf-8
            return xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        }
    }
}