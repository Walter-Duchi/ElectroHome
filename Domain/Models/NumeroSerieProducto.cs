using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class NumeroSerieProducto
{
    public int Id { get; set; }

    public int FkProducto { get; set; }

    public string NumeroSerie { get; set; } = null!;

    public bool? Vendido { get; set; }

    public virtual DetallesCompra? DetallesCompra { get; set; }

    public virtual Producto FkProductoNavigation { get; set; } = null!;
}
