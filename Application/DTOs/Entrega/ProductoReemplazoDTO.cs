namespace Application.DTOs.Entrega
{
    public class ProductoReemplazoDTO
    {
        public int Id { get; set; }
        public string NumeroSerie { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string EstadoInventario { get; set; } = string.Empty;
    }
}