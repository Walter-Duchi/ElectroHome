using SriAutorizacion;
using SriRecepcion;
using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Infrastructure.WcfInspectors;

namespace Infrastructure.Facturacion.Services
{
    public interface ISriFacturacionService
    {
        Task<SriRecepcion.validarComprobanteResponse> EnviarComprobante(byte[] xmlFirmado);
        Task<SriAutorizacion.autorizacionComprobanteResponse> ConsultarAutorizacion(string claveAcceso);
    }

    public class SriFacturacionService : ISriFacturacionService
    {
        private readonly RecepcionComprobantesOfflineClient _recepcionClient;
        private readonly AutorizacionComprobantesOfflineClient _autorizacionClient;

        public SriFacturacionService()
        {
            _recepcionClient = new RecepcionComprobantesOfflineClient(
                RecepcionComprobantesOfflineClient.EndpointConfiguration.RecepcionComprobantesOfflinePort,
                "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline"
            );
            _autorizacionClient = new AutorizacionComprobantesOfflineClient(
                AutorizacionComprobantesOfflineClient.EndpointConfiguration.AutorizacionComprobantesOfflinePort,
                "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline"
            );

            // Agregar inspector para capturar las respuestas
            _recepcionClient.Endpoint.EndpointBehaviors.Add(new InspectorBehavior());
            _autorizacionClient.Endpoint.EndpointBehaviors.Add(new InspectorBehavior());
        }

        public async Task<SriRecepcion.validarComprobanteResponse> EnviarComprobante(byte[] xmlFirmado)
        {
            Console.WriteLine("Enviando comprobante al SRI...");
            var request = new SriRecepcion.validarComprobante(xmlFirmado);
            try
            {
                var response = await _recepcionClient.validarComprobanteAsync(request);

                // Intentar serializar la respuesta completa para depuración
                try
                {
                    var serializer = new XmlSerializer(typeof(SriRecepcion.validarComprobanteResponse));
                    using (var writer = new StringWriter())
                    {
                        serializer.Serialize(writer, response);
                        Console.WriteLine("=== RESPUESTA COMPLETA DEL SRI ===");
                        Console.WriteLine(writer.ToString());
                        Console.WriteLine("===================================");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("No se pudo serializar la respuesta: " + ex.Message);
                }

                if (response?.RespuestaRecepcionComprobante == null)
                {
                    Console.WriteLine("La respuesta del SRI es nula o no contiene la estructura esperada.");
                    // Return a new instance to avoid returning null
                    return new SriRecepcion.validarComprobanteResponse();
                }
                else
                {
                    Console.WriteLine($"Respuesta del SRI - Estado: {response.RespuestaRecepcionComprobante.estado ?? "null"}");
                    if (response.RespuestaRecepcionComprobante.comprobantes?.Length > 0)
                    {
                        foreach (var comp in response.RespuestaRecepcionComprobante.comprobantes)
                        {
                            Console.WriteLine($"Comprobante: {comp.claveAcceso}, mensajes: {comp.mensajes?.Length}");
                            if (comp.mensajes != null)
                            {
                                foreach (var msg in comp.mensajes)
                                {
                                    Console.WriteLine($"  Mensaje: {msg.identificador} - {msg.mensaje1} - {msg.informacionAdicional} - {msg.tipo}");
                                }
                            }
                        }
                    }
                }
                return response!;
            }
            catch (System.ServiceModel.FaultException ex)
            {
                Console.WriteLine($"FaultException al enviar comprobante: {ex.Message}");
                try
                {
                    var detail = ex.CreateMessageFault()?.GetDetail<string>();
                    Console.WriteLine($"Detalle: {detail}");
                }
                catch { }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar comprobante: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                throw;
            }
        }

        public async Task<SriAutorizacion.autorizacionComprobanteResponse> ConsultarAutorizacion(string claveAcceso)
        {
            var request = new SriAutorizacion.autorizacionComprobante(claveAcceso);
            return await _autorizacionClient.autorizacionComprobanteAsync(request);
        }
    }
}