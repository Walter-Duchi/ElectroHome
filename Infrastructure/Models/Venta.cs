using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Venta
{
    public int Id { get; set; }

    public string CodigoFactura { get; set; } = null!;

    public int FkEmpresaCliente { get; set; }

    public DateTime? FechaCompra { get; set; }

    public decimal TotalCompra { get; set; }

    public virtual Usuario FkEmpresaClienteNavigation { get; set; } = null!;

    public virtual ICollection<VentasPorNumeroSerieProducto> VentasPorNumeroSerieProductos { get; set; } = new List<VentasPorNumeroSerieProducto>();
}
