using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ProductoImpuesto
{
    public int Id { get; set; }

    public int FkProducto { get; set; }

    public int FkImpuesto { get; set; }

    public decimal PorcentajeAplicado { get; set; }

    public DateOnly? FechaVigencia { get; set; }

    public bool? Activo { get; set; }

    public virtual ImpuestosConfiguracion FkImpuestoNavigation { get; set; } = null!;

    public virtual Producto FkProductoNavigation { get; set; } = null!;
}
