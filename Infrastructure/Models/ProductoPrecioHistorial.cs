namespace Infrastructure.Models;

public partial class ProductoPrecioHistorial
{
    public int Id { get; set; }

    public int FkProducto { get; set; }

    public decimal PrecioAnterior { get; set; }

    public decimal PrecioNuevo { get; set; }

    public int? FkUsuario { get; set; }

    public DateTime? FechaCambio { get; set; }

    public string? Motivo { get; set; }

    public virtual Producto FkProductoNavigation { get; set; } = null!;

    public virtual Usuario? FkUsuarioNavigation { get; set; }
}
