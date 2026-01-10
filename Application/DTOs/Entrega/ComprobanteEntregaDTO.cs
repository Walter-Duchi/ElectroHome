namespace Application.DTOs.Entrega
{
    public class ComprobanteEntregaDTO
    {
        public string CodigoReclamo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public DateTime FechaEntrega { get; set; }
        public string PersonalEntrega { get; set; } = string.Empty;
        public List<ProductoEntregaComprobanteDTO> Productos { get; set; } = new();
        public string FirmaBase64 { get; set; } = string.Empty;
    }

    public class ProductoEntregaComprobanteDTO
    {
        public string NumeroSerieDefectuoso { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerieReemplazo { get; set; } = string.Empty;
    }
}