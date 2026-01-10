namespace Application.DTOs.Entrega
{
    public class ProductoEntregaDTO
    {
        public int ReclamoProductoSnId { get; set; }
        public string NumeroSerieProductoDefectuoso { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string FormaCompensacion { get; set; } = string.Empty;
        public string? NumeroSerieReemplazo { get; set; }
        public int? ProductoReemplazoId { get; set; }
        public bool ReemplazoValido { get; set; }
        public string? MensajeValidacion { get; set; }
    }
}