namespace Infrastructure.Models;

public partial class ReclamosProductoSn
{
    public int Id { get; set; }

    public int FkNumeroSerieProductos { get; set; }

    public int FkReclamos { get; set; }

    public DateTime FechaVentaClienteFinal { get; set; }

    public DateTime? FechaReclamoClienteFinal { get; set; }

    public string FormaCompensacion { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public int? FkTecnicoAsignado { get; set; }

    public DateTime? FechaRevisionTecnico { get; set; }

    public string? ExplicacionRespuestaTecnico { get; set; }

    public string? PdfRevisionTecnico { get; set; }

    public virtual ComprobanteProductoReemplazado? ComprobanteProductoReemplazado { get; set; }

    public virtual NumeroSerieProducto FkNumeroSerieProductosNavigation { get; set; } = null!;

    public virtual Reclamo FkReclamosNavigation { get; set; } = null!;

    public virtual Usuario? FkTecnicoAsignadoNavigation { get; set; }

    public virtual ReembolsoPorReclamo? ReembolsoPorReclamo { get; set; }
}
