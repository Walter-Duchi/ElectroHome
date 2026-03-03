namespace Application.DTOs.Ecommerce
{
    public class ProductFilterRequest
    {
        public string? Busqueda { get; set; }
        public int? CategoriaId { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public string? OrdenarPor { get; set; }  // "popular", "precio_asc", "precio_desc", "nuevos"
    }
}