using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class NumeroSerieProducto
{
    public int Id { get; set; }

    public int FkProducto { get; set; }

    public string NumeroSerie { get; set; } = null!;

    public string? EstadoInventario { get; set; }

    public virtual ComprobanteProductoReemplazado? ComprobanteProductoReemplazado { get; set; }

    public virtual Producto FkProductoNavigation { get; set; } = null!;

    public virtual ICollection<MarcaLoEntregoComoReemplazo> MarcaLoEntregoComoReemplazos { get; set; } = new List<MarcaLoEntregoComoReemplazo>();

    public virtual ReclamosProductoSn? ReclamosProductoSn { get; set; }

    public virtual VentasPorNumeroSerieProducto? VentasPorNumeroSerieProducto { get; set; }
}
