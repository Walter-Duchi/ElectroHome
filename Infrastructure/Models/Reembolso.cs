using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Reembolso
{
    public int Id { get; set; }

    public string? NumeroComprobanteReembolso { get; set; }

    public DateTime? FechaReembolso { get; set; }

    public virtual ICollection<ReembolsoReclamosProducto> ReembolsoReclamosProductos { get; set; } = new List<ReembolsoReclamosProducto>();
}
