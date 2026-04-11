namespace Application.DTOs.Reclamos.Entrega
{
    public class ReclamoPendienteEntregaDTO
    {
        public int Id { get; set; }
        public string CodigoReclamo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int CantidadProductosPendientes { get; set; }
    }
}