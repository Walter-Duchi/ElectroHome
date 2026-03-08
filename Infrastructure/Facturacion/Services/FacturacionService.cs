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
using Infrastructure.WcfInspectors; // Añadido para acceder a MessageInspectorStorage

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
                            codigoPorcentaje = "4", // 15% (código según tabla 17)
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
                        new CampoAdicional { Nombre = "Email", Valor = venta.FkEmpresaClienteNavigation.Correo ?? "" },
                        new CampoAdicional { Nombre = "Teléfono", Valor = venta.FkEmpresaClienteNavigation.Celular ?? "" }
                    }
                }
            };

            // Generar clave de acceso
            factura.InfoTributaria.claveAcceso = ClaveAccesoHelper.GenerarClaveAcceso(
                venta.FechaCompra ?? throw new InvalidOperationException("Fecha de compra no puede ser nula"),
                factura.InfoTributaria.ruc,
                factura.InfoTributaria.estab,
                factura.InfoTributaria.ptoEmi,
                secuencial
            );

            // Serializar a XML
            var xmlSinFirma = SerializarAFacturaXml(factura);

            // Firmar electrónicamente
            var xmlFirmado = await _firmaService.FirmarXmlAsync(xmlSinFirma);

            // Guardar XML firmado para inspección manual (opcional)
            File.WriteAllText("C:/temp/xmlFirmado.xml", xmlFirmado);

            // Enviar al SRI
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
                venta.EstadoSri = "Rechazado";
                await _context.SaveChangesAsync();
                throw;
            }

            // Capturar el XML crudo de la respuesta de recepción (guardado por el inspector)
            string rawRecepcionXml = MessageInspectorStorage.LastResponseXml;
            // Limpiar el almacenamiento para no interferir con futuras llamadas
            MessageInspectorStorage.LastResponseXml = null;

            // --- Procesar respuesta de recepción ---
            // Primero, verificar manualmente si existe el error 45 (ERROR SECUENCIAL REGISTRADO)
            bool esErrorSecuencialRegistrado = false;
            if (!string.IsNullOrEmpty(rawRecepcionXml))
            {
                try
                {
                    var doc = XDocument.Parse(rawRecepcionXml);
                    // Buscar cualquier elemento <mensaje> que contenga <identificador>45</identificador>
                    var mensajes = doc.Descendants().Where(x => x.Name.LocalName == "mensaje");
                    foreach (var msg in mensajes)
                    {
                        var identificador = msg.Element(XName.Get("identificador"))?.Value;
                        if (identificador == "45")
                        {
                            esErrorSecuencialRegistrado = true;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al parsear XML crudo de recepción para detectar error 45.");
                }
            }

            if (esErrorSecuencialRegistrado)
            {
                _logger.LogWarning("Detectado ERROR SECUENCIAL REGISTRADO (identificador 45) en la respuesta de recepción para la venta {VentaId}. La venta ya fue facturada previamente.", ventaId);

                // Verificar el estado actual en base de datos
                if (venta.EstadoSri == "Rechazado")
                {
                    // Inconsistencia: la BD dice rechazado pero el SRI indica que ya fue registrada (debería estar autorizada)
                    _logger.LogWarning("Corrigiendo inconsistencia: venta {VentaId} estaba en estado Rechazado, se actualiza a Autorizado.", ventaId);
                    venta.EstadoSri = "Autorizado";
                    await _context.SaveChangesAsync();
                }
                // Si ya está Autorizado, no hacer nada (no se modifica la BD)

                // Lanzar excepción con mensaje claro para que la API lo devuelva
                throw new Exception($"La venta {ventaId} ya fue facturada y autorizada previamente. No se puede facturar nuevamente.");
            }

            // Si no es error 45, continuar con el flujo normal
            if (respuestaRecepcion?.RespuestaRecepcionComprobante?.estado == "RECIBIDA")
            {
                try
                {
                    await Task.Delay(3000);
                    var respuestaAutorizacion = await _sriService.ConsultarAutorizacion(factura.InfoTributaria.claveAcceso);

                    // --- Procesamiento con objeto deserializado ---
                    bool autorizacionProcesada = false;
                    if (respuestaAutorizacion.RespuestaDeserializada?.RespuestaAutorizacionComprobante != null)
                    {
                        var comprobante = respuestaAutorizacion.RespuestaDeserializada.RespuestaAutorizacionComprobante;
                        _logger.LogInformation("Respuesta deserializada: ClaveConsultada={Clave}, NumComprobantes={Num}, Autorizaciones.Length={Len}",
                            comprobante.claveAccesoConsultada, comprobante.numeroComprobantes, comprobante.autorizaciones?.Length ?? 0);

                        if (comprobante.autorizaciones != null && comprobante.autorizaciones.Length > 0)
                        {
                            var autorizacion = comprobante.autorizaciones[0];
                            _logger.LogInformation("Autorización encontrada por deserialización: Estado={Estado}, Numero={Numero}",
                                autorizacion.estado, autorizacion.numeroAutorizacion);

                            venta.EstadoSri = autorizacion.estado == "AUTORIZADO" ? "Autorizado" : "Rechazado";
                            venta.FechaAutorizacion = DateTime.Now;
                            venta.ClaveAcceso = autorizacion.numeroAutorizacion;

                            if (!string.IsNullOrEmpty(autorizacion.comprobante))
                            {
                                var rutaXml = Path.Combine(_configuration["RutaXmlAutorizados"] ?? "XmlAutorizados",
                                                           $"{factura.InfoTributaria.claveAcceso}.xml");
                                Directory.CreateDirectory(Path.GetDirectoryName(rutaXml)!);
                                await File.WriteAllTextAsync(rutaXml, autorizacion.comprobante);
                                venta.XmlPath = rutaXml;
                                _logger.LogInformation("XML autorizado guardado en {Ruta}", rutaXml);
                            }
                            else
                            {
                                _logger.LogWarning("El XML autorizado viene vacío, se guardará el firmado original.");
                                var rutaXml = Path.Combine(_configuration["RutaXmlAutorizados"] ?? "XmlAutorizados",
                                                           $"{factura.InfoTributaria.claveAcceso}.xml");
                                Directory.CreateDirectory(Path.GetDirectoryName(rutaXml)!);
                                await File.WriteAllTextAsync(rutaXml, xmlFirmado);
                                venta.XmlPath = rutaXml;
                            }
                            autorizacionProcesada = true;
                        }
                    }

                    // --- Si no se pudo por deserialización, intentar parseo manual del XML crudo (ignorando namespaces) ---
                    if (!autorizacionProcesada)
                    {
                        _logger.LogWarning("No se pudo obtener autorización por deserialización. Intentando parseo manual del XML crudo.");

                        string xmlParaParsear = respuestaAutorizacion.XmlRespuestaCruda;
                        if (!string.IsNullOrEmpty(xmlParaParsear))
                        {
                            try
                            {
                                var doc = System.Xml.Linq.XDocument.Parse(xmlParaParsear);
                                // Buscar elemento <autorizacion> por nombre local (sin namespace)
                                var authElement = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "autorizacion");

                                if (authElement != null)
                                {
                                    var estado = (string)authElement.Element(XName.Get("estado"));
                                    var numero = (string)authElement.Element(XName.Get("numeroAutorizacion"));
                                    var fechaStr = (string)authElement.Element(XName.Get("fechaAutorizacion"));
                                    var comprobanteXml = (string)authElement.Element(XName.Get("comprobante"));

                                    _logger.LogInformation("Parseo manual exitoso: Estado={Estado}, Numero={Numero}", estado, numero);

                                    venta.EstadoSri = estado == "AUTORIZADO" ? "Autorizado" : "Rechazado";
                                    if (DateTime.TryParse(fechaStr, out var fecha))
                                        venta.FechaAutorizacion = fecha;
                                    else
                                        venta.FechaAutorizacion = DateTime.Now;
                                    venta.ClaveAcceso = numero;

                                    if (!string.IsNullOrEmpty(comprobanteXml))
                                    {
                                        var rutaXml = Path.Combine(_configuration["RutaXmlAutorizados"] ?? "XmlAutorizados",
                                                                   $"{factura.InfoTributaria.claveAcceso}.xml");
                                        Directory.CreateDirectory(Path.GetDirectoryName(rutaXml)!);
                                        await File.WriteAllTextAsync(rutaXml, comprobanteXml);
                                        venta.XmlPath = rutaXml;
                                        _logger.LogInformation("XML autorizado guardado en {Ruta} (parseo manual)", rutaXml);
                                    }
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
                        _logger.LogWarning("No se pudo obtener ninguna autorización para la clave {Clave}", factura.InfoTributaria.claveAcceso);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al consultar autorización para la clave {Clave}", factura.InfoTributaria.claveAcceso);
                    venta.EstadoSri = "Rechazado";
                }
            }
            else // estado != RECIBIDA (por ejemplo DEVUELTA)
            {
                venta.EstadoSri = "Rechazado";
                if (respuestaRecepcion?.RespuestaRecepcionComprobante?.comprobantes?.Length > 0)
                {
                    var errores = respuestaRecepcion.RespuestaRecepcionComprobante.comprobantes[0].mensajes;
                    _logger.LogError("Errores en recepción: {@errores}", errores);

                    // Verificar si es error por duplicado (identificador 45) - aunque ya lo detectamos arriba, por si acaso
                    if (errores != null && errores.Any(m => m.identificador == "45"))
                    {
                        throw new Exception("La venta ya ha sido facturada previamente.");
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Venta {VentaId} actualizada con estado {Estado}", ventaId, venta.EstadoSri);
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