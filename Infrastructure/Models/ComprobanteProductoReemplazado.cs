namespace Infrastructure.Models;

public partial class ComprobanteProductoReemplazado
{
    public int Id { get; set; }

    public int ReclamosProductoSn { get; set; }

    public int FkComprobanteDeReemplazo { get; set; }

    public int FkProductoDeReemplazo { get; set; }

    public virtual ComprobanteDeReemplazo FkComprobanteDeReemplazoNavigation { get; set; } = null!;

    public virtual NumeroSerieProducto FkProductoDeReemplazoNavigation { get; set; } = null!;

    public virtual ReclamosProductoSn ReclamosProductoSnNavigation { get; set; } = null!;
}
