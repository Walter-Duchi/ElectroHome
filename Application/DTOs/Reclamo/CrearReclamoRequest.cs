namespace Application.DTOs.Reclamo
{
    public class CrearReclamoRequest
    {
        public string RucCliente { get; set; } = null!;
        public List<ProductoReclamadoRequest> Productos { get; set; } = new();
    }
}