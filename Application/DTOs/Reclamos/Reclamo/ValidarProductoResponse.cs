namespace Application.DTOs.Reclamos.Reclamo
{
    public class ValidarProductoResponse
    {
        public bool EsValido { get; set; }
        public string? Mensaje { get; set; }
        public int? ProductoId { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public bool TieneGarantia { get; set; }
        public string? EstadoInventario { get; set; }
        public DateTime? FechaVenta { get; set; }
        public int? DiasGarantia { get; set; }
        public string? Especificacion { get; set; }
        public decimal? Precio { get; set; }
    }
}