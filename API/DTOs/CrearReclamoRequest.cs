using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public record CrearReclamoRequest
    (
        [Required] string NumeroSerieProducto,
        [Required]  string FormaDeCompensacion
    );
}
