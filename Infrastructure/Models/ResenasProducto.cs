namespace Infrastructure.Models;

public partial class ResenasProducto
{
    public int Id { get; set; }

    public int FkCliente { get; set; }

    public int FkProducto { get; set; }

    public int? Calificacion { get; set; }

    public string? Comentario { get; set; }

    public DateTime? FechaResena { get; set; }

    public string? Estado { get; set; }

    public virtual Usuario FkClienteNavigation { get; set; } = null!;

    public virtual Producto FkProductoNavigation { get; set; } = null!;
}
