namespace Application.DTOs.Ecommerce
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? CategoriaPadreId { get; set; }
    }
}