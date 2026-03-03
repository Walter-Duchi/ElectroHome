namespace Infrastructure.Models;

public partial class MetodosPago
{
    public int Id { get; set; }

    public string Tipo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public bool? RequiereConfirmacion { get; set; }

    public decimal? ComisionPorcentaje { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual ICollection<Reembolso> Reembolsos { get; set; } = new List<Reembolso>();
}
