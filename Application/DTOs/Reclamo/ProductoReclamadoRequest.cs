namespace Application.DTOs.Reclamo
{
    public class ProductoReclamadoRequest
    {
        public string NumeroSerie { get; set; } = null!;
        public string FormaCompensacion { get; set; } = null!;
    }
}