using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ImpuestoArancelario
{
    public int Id { get; set; }

    public string Pais { get; set; } = null!;

    public decimal Porcentaje { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
