namespace Infrastructure.Models;

public partial class VentasPorNumeroSerieProducto
{
    public int Id { get; set; }

    public int FkVentas { get; set; }

    public int FkNumeroSerieProducto { get; set; }

    public decimal PrecioVenta { get; set; }

    public virtual NumeroSerieProducto FkNumeroSerieProductoNavigation { get; set; } = null!;

    public virtual Venta FkVentasNavigation { get; set; } = null!;
}
