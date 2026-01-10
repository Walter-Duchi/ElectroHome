using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Entrega
{
    public class ConfirmarEntregaRequest
    {
        [Required(ErrorMessage = "El código de reclamo es obligatorio")]
        public string CodigoReclamo { get; set; } = string.Empty;
    }
}