using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Proveedore
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Cedula { get; set; } = null!;

    public string Ruc { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? ContactoPrincipal { get; set; }

    public int? PlazoEntregaDias { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<NumeroSerieProducto> NumeroSerieProductos { get; set; } = new List<NumeroSerieProducto>();
}
