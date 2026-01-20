using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Reclamos.Entrega
{
    public class SubirComprobanteRequest
    {
        [Required(ErrorMessage = "El código de reclamo es obligatorio")]
        public string CodigoReclamo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El PDF firmado es obligatorio")]
        public string PdfBase64 { get; set; } = string.Empty;
    }
}