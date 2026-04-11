namespace Application.DTOs.Reclamos.Reclamo
{
    public class CrearReclamoRequest
    {
        public string IdentificadorCliente { get; set; } = null!;
        public List<ProductoReclamadoRequest> Productos { get; set; } = new();
    }
}