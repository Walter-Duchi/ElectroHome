namespace Infrastructure.Models;

public partial class ComprobanteDeReemplazo
{
    public int Id { get; set; }

    public string? PdfComprobanteEntregaCliente { get; set; }

    public virtual ICollection<ComprobanteProductoReemplazado> ComprobanteProductoReemplazados { get; set; } = new List<ComprobanteProductoReemplazado>();
}
