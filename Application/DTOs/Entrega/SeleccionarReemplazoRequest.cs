using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Entrega
{
    public class SeleccionarReemplazoRequest
    {
        [Required(ErrorMessage = "El ID del producto en reclamo es obligatorio")]
        public int ReclamoProductoSnId { get; set; }

        [Required(ErrorMessage = "El número de serie del producto de reemplazo es obligatorio")]
        [StringLength(50, ErrorMessage = "El número de serie no puede exceder 50 caracteres")]
        public string NumeroSerieReemplazo { get; set; } = string.Empty;
    }
}