using System.Threading.Tasks;
using SriRecepcion;
using SriAutorizacion;

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
        }

        public async Task<SriRecepcion.validarComprobanteResponse> EnviarComprobante(byte[] xmlFirmado)
        {
            Console.WriteLine("Enviando comprobante al SRI...");
            var request = new SriRecepcion.validarComprobante(xmlFirmado);
            try
            {
                var response = await _recepcionClient.validarComprobanteAsync(request);
                Console.WriteLine($"Respuesta del SRI: {response.RespuestaRecepcionComprobante.estado}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar comprobante: {ex.Message}");
                throw; // o manejar según tu lógica
            }
        }

        public async Task<SriAutorizacion.autorizacionComprobanteResponse> ConsultarAutorizacion(string claveAcceso)
        {
            var request = new SriAutorizacion.autorizacionComprobante(claveAcceso);
            return await _autorizacionClient.autorizacionComprobanteAsync(request);
        }
    }
}