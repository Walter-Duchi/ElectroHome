namespace Infrastructure.Models;

public partial class InventarioUbicacione
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Tipo { get; set; }

    public int? FkUbicacionPadre { get; set; }

    public int? CapacidadMaxima { get; set; }

    public bool? Activo { get; set; }

    public virtual InventarioUbicacione? FkUbicacionPadreNavigation { get; set; }

    public virtual ICollection<InventarioUbicacione> InverseFkUbicacionPadreNavigation { get; set; } = new List<InventarioUbicacione>();

    public virtual ICollection<NumeroSerieProducto> NumeroSerieProductos { get; set; } = new List<NumeroSerieProducto>();
}
