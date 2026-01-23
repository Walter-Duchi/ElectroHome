using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ImpuestosConfiguracion
{
    public int Id { get; set; }

    public string CodigoImpuesto { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public decimal Porcentaje { get; set; }

    public string? AplicableA { get; set; }

    public DateOnly FechaVigenciaInicio { get; set; }

    public DateOnly? FechaVigenciaFin { get; set; }

    public bool? Activo { get; set; }

    public int? CreadoPor { get; set; }

    public int? ModificadoPor { get; set; }

    public virtual Usuario? CreadoPorNavigation { get; set; }

    public virtual Usuario? ModificadoPorNavigation { get; set; }

    public virtual ICollection<ProductoImpuesto> ProductoImpuestos { get; set; } = new List<ProductoImpuesto>();
}
