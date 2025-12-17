namespace Infrastructure.Models;

public partial class ComprasProducto
{
    public int Id { get; set; }

    public int FkCompra { get; set; }

    public string FkNumeroSerie { get; set; } = null!;

    public decimal PrecioVenta { get; set; }

    public virtual Compra FkCompraNavigation { get; set; } = null!;

    public virtual NumeroSerieProducto FkNumeroSerieNavigation { get; set; } = null!;

    public virtual ReclamosProducto? ReclamosProducto { get; set; }
}
