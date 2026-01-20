namespace Application.DTOs.Reclamos.Entrega
{
    public class ValidarReemplazoResponse
    {
        public bool Valido { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public ProductoReemplazoDTO? ProductoReemplazo { get; set; }
    }
}