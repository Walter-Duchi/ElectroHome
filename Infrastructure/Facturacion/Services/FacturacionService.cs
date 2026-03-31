using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
using Infrastructure.WcfInspectors;

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
        private const int MAX_INTENTOS_POST = 3;
        private const int ESPERA_ENTRE_INTENTOS_MS = 3000;

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
            await FacturarVentaConReintento(ventaId, 1);
        }

        private async Task FacturarVentaConReintento(int ventaId, int intento)
        {
            _logger.LogInformation("=== INTENTO {Intento} de facturación para venta {VentaId} ===", intento, ventaId);

            var venta = await _context.Ventas
                .Include(v => v.FkEmpresaClienteNavigation)
                .Include(v => v.VentasPorNumeroSerieProductos)
                    .ThenInclude(vp => vp.FkNumeroSerieProductoNavigation)
                        .ThenInclude(nsp => nsp.FkProductoNavigation)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
                throw new Exception("Venta no encontrada");

            var secuencial = venta.CodigoFactura.Split('-').Last().PadLeft(9, '0');

            var detallesFactura = venta.VentasPorNumeroSerieProductos.Select(vp => new
            {
                BaseImponible = vp.PrecioVenta - vp.Iva,
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

            var cliente = venta.FkEmpresaClienteNavigation;

            // Determinar tipo de identificación según la longitud del campo Identificacion
            string tipoIdentificacion;
            string identificacion = cliente.Identificacion ?? "";
            string razonSocial;

            if (identificacion.Length == 13)
            {
                tipoIdentificacion = "04"; // RUC
                razonSocial = cliente.RazonSocial ?? $"{cliente.Nombres} {cliente.Apellidos}";
            }
            else if (identificacion.Length == 10)
            {
                tipoIdentificacion = "05"; // Cédula
                razonSocial = $"{cliente.Nombres} {cliente.Apellidos}";
            }
            else
            {
                tipoIdentificacion = "05"; // Consumidor final
                razonSocial = "CONSUMIDOR FINAL";
                identificacion = "9999999999999";
            }

            // Si no hay identificación, usar consumidor final
            if (string.IsNullOrEmpty(identificacion))
            {
                tipoIdentificacion = "05";
                identificacion = "9999999999999";
                razonSocial = "CONSUMIDOR FINAL";
            }

            var fechaEmision = venta.FechaCompra?.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)
                                ?? DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var factura = new Factura
            {
                InfoTributaria = new InfoTributaria
                {
                    ambiente = "1",
                    tipoEmision = "1",
                    razonSocial = "DUCHI RIVERA WALTER ALEJANDRO",
                    nombreComercial = "SOFTWARE HOME",
                    ruc = "0950734061001",
                    claveAcceso = "",
                    codDoc = "01",
                    estab = "001",
                    ptoEmi = "001",
                    secuencial = secuencial,
                    dirMatriz = "RIO AGUARICO Y RIO PASTAZA 123 Y CALLE B, MILAGRO",
                    contribuyenteRimpe = "CONTRIBUYENTE NEGOCIO POPULAR - RÉGIMEN RIMPE",
                },
                InfoFactura = new InfoFactura
                {
                    fechaEmision = fechaEmision,
                    dirEstablecimiento = "RIO AGUARICO Y RIO PASTAZA 123 Y CALLE B, MILAGRO",
                    obligadoContabilidad = "NO",
                    tipoIdentificacionComprador = tipoIdentificacion,
                    razonSocialComprador = razonSocial,
                    identificacionComprador = identificacion,
                    direccionComprador = cliente.Direccion ?? "",
                    totalSinImpuestos = Math.Round(totalSinImpuestos, 2),
                    totalDescuento = Math.Round(totalDescuento, 2),
                    totalConImpuestos = new System.Collections.Generic.List<TotalImpuesto>
                    {
                        new TotalImpuesto
                        {
                            codigo = "2",
                            codigoPorcentaje = "4",
                            baseImponible = Math.Round(totalSinImpuestos, 2),
                            valor = Math.Round(totalIva, 2)
                        }
                    },
                    propina = 0,
                    importeTotal = venta.TotalCompra,
                    moneda = "DOLAR",
                    pagos = new System.Collections.Generic.List<PagoFactura>
                    {
                        new PagoFactura
                        {
                            formaPago = "01",
                            total = venta.TotalCompra
                        }
                    }
                },
                Detalles = venta.VentasPorNumeroSerieProductos.Select(vp => new Detalle
                {
                    CodigoPrincipal = vp.FkNumeroSerieProductoNavigation.FkProductoNavigation.Sku ?? "",
                    descripcion = vp.FkNumeroSerieProductoNavigation.FkProductoNavigation.Descripcion ?? "",
                    cantidad = 1,
                    precioUnitario = Math.Round((vp.PrecioVenta - vp.Iva) + (vp.Descuento ?? 0), 2),
                    descuento = Math.Round(vp.Descuento ?? 0, 2),
                    precioTotalSinImpuesto = Math.Round(vp.PrecioVenta - vp.Iva, 2),
                    impuestos = new System.Collections.Generic.List<DetalleImpuesto>
                    {
                        new DetalleImpuesto
                        {
                            codigo = "2",
                            codigoPorcentaje = "4",
                            tarifa = 15,
                            baseImponible = Math.Round(vp.PrecioVenta - vp.Iva, 2),
                            valor = Math.Round(vp.Iva, 2)
                        }
                    }
                }).ToList(),
                InfoAdicional = new InfoAdicional
                {
                    Campos = new System.Collections.Generic.List<CampoAdicional>
                    {
                        new CampoAdicional { Nombre = "Email", Valor = cliente.Correo ?? "" },
                        new CampoAdicional { Nombre = "Teléfono", Valor = cliente.Celular ?? "" }
                    }
                }
            };

            factura.InfoTributaria.claveAcceso = ClaveAccesoHelper.GenerarClaveAcceso(
                venta.FechaCompra ?? throw new InvalidOperationException("Fecha de compra no puede ser nula"),
                factura.InfoTributaria.ruc,
                factura.InfoTributaria.estab,
                factura.InfoTributaria.ptoEmi,
                secuencial
            );

            var xmlSinFirma = SerializarAFacturaXml(factura);
            var xmlFirmado = await _firmaService.FirmarXmlAsync(xmlSinFirma);

            var bytesXml = Encoding.UTF8.GetBytes(xmlFirmado);
            SriRecepcion.validarComprobanteResponse respuestaRecepcion;

            try
            {
                respuestaRecepcion = await _sriService.EnviarComprobante(bytesXml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar comprobante al SRI");
                venta.EstadoSri = "Rechazado";
                await _context.SaveChangesAsync();
                throw;
            }

            string rawRecepcionXml = MessageInspectorStorage.LastResponseXml;
            MessageInspectorStorage.LastResponseXml = null;

            bool esErrorSecuencialRegistrado = false;
            bool esClaveEnProcesamiento = false;
            if (!string.IsNullOrEmpty(rawRecepcionXml))
            {
                try
                {
                    var doc = XDocument.Parse(rawRecepcionXml);
                    var mensajes = doc.Descendants().Where(x => x.Name.LocalName == "mensaje");
                    foreach (var msg in mensajes)
                    {
                        var identificador = msg.Element(XName.Get("identificador"))?.Value;
                        if (identificador == "45")
                        {
                            esErrorSecuencialRegistrado = true;
                            break;
                        }
                        if (identificador == "70")
                        {
                            esClaveEnProcesamiento = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al parsear XML crudo de recepción.");
                }
            }

            if (esErrorSecuencialRegistrado)
            {
                _logger.LogWarning("Detectado ERROR SECUENCIAL REGISTRADO (identificador 45) para la venta {VentaId}. La venta ya fue facturada previamente.", ventaId);
                if (venta.EstadoSri == "Rechazado")
                {
                    venta.EstadoSri = "Autorizado";
                    await _context.SaveChangesAsync();
                }
                throw new Exception($"La venta {ventaId} ya fue facturada y autorizada previamente. No se puede facturar nuevamente.");
            }

            if (esClaveEnProcesamiento)
            {
                _logger.LogInformation("Detectado CLAVE DE ACCESO EN PROCESAMIENTO (identificador 70) para la venta {VentaId}. Intento {Intento} de {MaxIntentos}.", ventaId, intento, MAX_INTENTOS_POST);
                if (intento < MAX_INTENTOS_POST)
                {
                    await Task.Delay(ESPERA_ENTRE_INTENTOS_MS);
                    await FacturarVentaConReintento(ventaId, intento + 1);
                    return;
                }
                else
                {
                    venta.EstadoSri = "Rechazado";
                    await _context.SaveChangesAsync();
                    throw new Exception("El comprobante fue recibido pero no se pudo obtener la autorización después de varios intentos de envío.");
                }
            }

            if (respuestaRecepcion?.RespuestaRecepcionComprobante?.estado == "RECIBIDA")
            {
                await Task.Delay(3000);
                var respuestaAutorizacion = await _sriService.ConsultarAutorizacion(factura.InfoTributaria.claveAcceso);

                bool autorizacionProcesada = false;
                if (respuestaAutorizacion.RespuestaDeserializada?.RespuestaAutorizacionComprobante != null)
                {
                    var comprobante = respuestaAutorizacion.RespuestaDeserializada.RespuestaAutorizacionComprobante;
                    if (comprobante.autorizaciones != null && comprobante.autorizaciones.Length > 0)
                    {
                        var autorizacion = comprobante.autorizaciones[0];
                        await ProcesarAutorizacion(venta, autorizacion, xmlFirmado, factura.InfoTributaria.claveAcceso);
                        autorizacionProcesada = true;
                    }
                }

                if (!autorizacionProcesada)
                {
                    _logger.LogWarning("No se pudo obtener autorización por deserialización. Intentando parseo manual del XML crudo.");
                    string xmlParaParsear = respuestaAutorizacion.XmlRespuestaCruda;
                    if (!string.IsNullOrEmpty(xmlParaParsear))
                    {
                        try
                        {
                            var doc = XDocument.Parse(xmlParaParsear);
                            var authElement = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "autorizacion");
                            if (authElement != null)
                            {
                                var estado = (string)authElement.Element(XName.Get("estado"));
                                var numero = (string)authElement.Element(XName.Get("numeroAutorizacion"));
                                var fechaStr = (string)authElement.Element(XName.Get("fechaAutorizacion"));
                                var comprobanteXml = (string)authElement.Element(XName.Get("comprobante"));
                                await ProcesarAutorizacionManual(venta, estado, numero, fechaStr, comprobanteXml, xmlFirmado, factura.InfoTributaria.claveAcceso);
                                autorizacionProcesada = true;
                            }
                            else
                            {
                                _logger.LogError("No se encontró elemento <autorizacion> en el XML crudo.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al parsear manualmente el XML crudo de autorización.");
                        }
                    }
                    else
                    {
                        _logger.LogError("No se pudo obtener el XML crudo para parseo manual.");
                    }
                }

                if (!autorizacionProcesada)
                {
                    venta.EstadoSri = "Rechazado";
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
            _logger.LogInformation("Venta {VentaId} actualizada con estado {Estado}", ventaId, venta.EstadoSri);
        }

        private async Task ProcesarAutorizacion(Venta venta, SriAutorizacion.autorizacion autorizacion, string xmlFirmado, string claveAccesoGenerada)
        {
            venta.EstadoSri = autorizacion.estado == "AUTORIZADO" ? "Autorizado" : "Rechazado";
            venta.FechaAutorizacion = DateTime.Now;
            venta.ClaveAcceso = autorizacion.numeroAutorizacion;

            if (!string.IsNullOrEmpty(autorizacion.comprobante))
            {
                venta.SriAutorizacion = autorizacion.comprobante;
                _logger.LogInformation("XML autorizado guardado en base de datos (clave {Clave})", claveAccesoGenerada);
            }
            else
            {
                _logger.LogWarning("El XML autorizado viene vacío, se guardará el firmado original.");
                venta.SriAutorizacion = xmlFirmado;
            }
        }

        private async Task ProcesarAutorizacionManual(Venta venta, string estado, string numero, string fechaStr, string comprobanteXml, string xmlFirmado, string claveAccesoGenerada)
        {
            venta.EstadoSri = estado == "AUTORIZADO" ? "Autorizado" : "Rechazado";
            if (DateTime.TryParse(fechaStr, out var fecha))
                venta.FechaAutorizacion = fecha;
            else
                venta.FechaAutorizacion = DateTime.Now;
            venta.ClaveAcceso = numero;

            if (!string.IsNullOrEmpty(comprobanteXml))
            {
                venta.SriAutorizacion = comprobanteXml;
                _logger.LogInformation("XML autorizado guardado en base de datos (parseo manual) para clave {Clave}", claveAccesoGenerada);
            }
            else
            {
                venta.SriAutorizacion = xmlFirmado;
            }
        }

        private string SerializarAFacturaXml(Factura factura)
        {
            var serializer = new XmlSerializer(typeof(Factura));
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, factura, ns);
            var xml = stringWriter.ToString();
            return xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        }
    }
}