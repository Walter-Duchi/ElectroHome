using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Reclamos.Entrega
{
    public class BuscarReclamoRequest
    {
        [Required(ErrorMessage = "El código de reclamo es obligatorio")]
        [StringLength(50, ErrorMessage = "El código de reclamo no puede exceder 50 caracteres")]
        public string CodigoReclamo { get; set; } = string.Empty;
    }
}