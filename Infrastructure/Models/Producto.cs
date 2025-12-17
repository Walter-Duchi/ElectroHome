namespace Infrastructure.Models;

public partial class Producto
{
    public int Id { get; set; }

    public int FkMarca { get; set; }

    public string Modelo { get; set; } = null!;

    public string Especificacion { get; set; } = null!;

    public int DiasGarantia { get; set; }

    public decimal Precio { get; set; }

    public virtual Marca FkMarcaNavigation { get; set; } = null!;

    public virtual ICollection<NumeroSerieProducto> NumeroSerieProductos { get; set; } = new List<NumeroSerieProducto>();
}
