namespace Infrastructure.Models;

public partial class ProductosPopularesCache
{
    public int Id { get; set; }

    public int FkProducto { get; set; }

    public int Posicion { get; set; }

    public int? VentasUltimos30Dias { get; set; }

    public int? VistasUltimos30Dias { get; set; }

    public decimal? RatioConversion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Producto FkProductoNavigation { get; set; } = null!;
}
