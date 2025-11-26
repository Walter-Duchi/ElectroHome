using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Compra
{
    public int Id { get; set; }

    public string CodigoFactura { get; set; } = null!;

    public int FkCliente { get; set; }

    public DateTime? FechaCompra { get; set; }

    public decimal TotalCompra { get; set; }

    public virtual ICollection<DetallesCompra> DetallesCompras { get; set; } = new List<DetallesCompra>();

    public virtual Usuario FkClienteNavigation { get; set; } = null!;
}
