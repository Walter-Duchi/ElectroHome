using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Reclamos.Entrega
{
    public class GenerarComprobanteRequest
    {
        [Required(ErrorMessage = "El código de reclamo es obligatorio")]
        public string CodigoReclamo { get; set; } = string.Empty;
    }
}