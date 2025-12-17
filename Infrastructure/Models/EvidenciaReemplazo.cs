namespace Infrastructure.Models;

public partial class EvidenciaReemplazo
{
    public int Id { get; set; }

    public string? PdfComprobanteEntregaCliente { get; set; }

    public virtual ICollection<EvidenciaReemplazoReclamosProducto> EvidenciaReemplazoReclamosProductos { get; set; } = new List<EvidenciaReemplazoReclamosProducto>();
}
