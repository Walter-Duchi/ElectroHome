namespace Infrastructure.Models;

public partial class ProductoImagene
{
    public int Id { get; set; }

    public int FkProducto { get; set; }

    public string UrlImagen { get; set; } = null!;

    public bool? EsPrincipal { get; set; }

    public virtual Producto FkProductoNavigation { get; set; } = null!;
}
