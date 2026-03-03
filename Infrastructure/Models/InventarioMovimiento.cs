namespace Infrastructure.Models;

public partial class InventarioMovimiento
{
    public int Id { get; set; }

    public int FkProducto { get; set; }

    public int? FkUsuario { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public int Cantidad { get; set; }

    public int CantidadAnterior { get; set; }

    public int CantidadNueva { get; set; }

    public string? Motivo { get; set; }

    public string? Referencia { get; set; }

    public DateTime? FechaMovimiento { get; set; }

    public decimal? CostoUnitario { get; set; }

    public virtual Producto FkProductoNavigation { get; set; } = null!;

    public virtual Usuario? FkUsuarioNavigation { get; set; }
}
