using System;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Yamgooo.SRI.Sign;

namespace Infrastructure.Facturacion.Services
{
    public interface IFirmaElectronicaService
    {
        /// <summary>
        /// Firma un XML sin firma usando el certificado configurado en appsettings.json
        /// </summary>
        /// <param name="xmlSinFirma">Contenido del XML a firmar</param>
        /// <returns>XML firmado</returns>
        Task<string> FirmarXmlAsync(string xmlSinFirma);

        /// <summary>
        /// Valida que un XML firmado tenga una firma electrónica válida
        /// </summary>
        /// <param name="xmlFirmado">XML con firma</param>
        /// <returns>True si la firma es válida</returns>
        bool ValidarFirma(string xmlFirmado);
    }

    public class FirmaElectronicaService : IFirmaElectronicaService
    {
        private readonly ISriSignService _sriSignService;
        private readonly ILogger<FirmaElectronicaService> _logger;

        public FirmaElectronicaService(ISriSignService sriSignService, ILogger<FirmaElectronicaService> logger)
        {
            _sriSignService = sriSignService;
            _logger = logger;
        }

        public async Task<string> FirmarXmlAsync(string xmlSinFirma)
        {
            try
            {
                // El certificado ya está configurado en appsettings.json (SriSign:CertificatePath y SriSign:CertificatePassword)
                var resultado = await _sriSignService.SignAsync(xmlSinFirma);

                if (resultado.Success)
                {
                    _logger.LogInformation("XML firmado correctamente en {0} ms", resultado.ProcessingTimeMs);
                    return resultado.SignedXml;
                }

                _logger.LogError("Error al firmar XML: {0}", resultado.ErrorMessage);
                throw new Exception($"Error en firma electrónica: {resultado.ErrorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción durante la firma del XML");
                throw;
            }
        }

        public bool ValidarFirma(string xmlFirmado)
        {
            try
            {
                return _sriSignService.ValidateSignature(xmlFirmado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar la firma del XML");
                return false;
            }
        }
    }
}