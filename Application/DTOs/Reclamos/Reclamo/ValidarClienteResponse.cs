namespace Application.DTOs.Reclamos.Reclamo
{
    public class ValidarClienteResponse
    {
        public bool EsValido { get; set; }
        public string? Mensaje { get; set; }
        public int? ClienteId { get; set; }
        public string? RazonSocial { get; set; }
    }
}