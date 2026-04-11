namespace Application.DTOs.Reclamos.Reclamo
{
    public class ProductoCompradoDTO
    {
        public string NumeroSerie { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public DateTime? FechaCompra { get; set; }
        public int DiasGarantia { get; set; }
        public bool TieneGarantia { get; set; }
    }
}