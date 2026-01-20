namespace Application.DTOs.Reclamos.Entrega
{
    public class BuscarReclamoResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string CodigoReclamo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public List<ProductoEntregaDTO> Productos { get; set; } = new();
        public bool TodosProductosRevisados { get; set; }
        public int TotalProductosReclamo { get; set; }
        public int ProductosPendientesRevision { get; set; }
    }
}