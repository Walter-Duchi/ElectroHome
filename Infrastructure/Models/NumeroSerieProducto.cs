using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class NumeroSerieProducto
{
    public int Id { get; set; }

    public int FkProducto { get; set; }

    public string NumeroSerie { get; set; } = null!;

    public bool? Vendido { get; set; }

    public virtual ComprasProducto? ComprasProducto { get; set; }

    public virtual Producto FkProductoNavigation { get; set; } = null!;
}
