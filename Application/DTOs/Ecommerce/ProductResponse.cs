namespace Application.DTOs.Ecommerce
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;  // Modelo + Marca
        public string Marca { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string? ImagenPrincipal { get; set; }  // URL de la imagen principal
        public List<string> ImagenesAdicionales { get; set; } = new();
        public int StockDisponible { get; set; }
        public int DiasGarantia { get; set; }
        public bool Activo { get; set; }
    }
}