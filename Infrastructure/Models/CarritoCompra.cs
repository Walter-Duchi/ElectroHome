namespace Infrastructure.Models;

public partial class CarritoCompra
{
    public int Id { get; set; }

    public int FkCliente { get; set; }

    public int FkProducto { get; set; }

    public int Cantidad { get; set; }

    public DateTime? FechaAgregado { get; set; }

    public virtual Usuario FkClienteNavigation { get; set; } = null!;

    public virtual Producto FkProductoNavigation { get; set; } = null!;
}
