using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class PayphoneTransaction
{
    public int Id { get; set; }

    public string ClientTransactionId { get; set; } = null!;

    public int FkUsuario { get; set; }

    public decimal MontoTotal { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public string DatosCarrito { get; set; } = null!;

    public long? PayphoneId { get; set; }

    public int? VentaId { get; set; }

    public virtual Usuario FkUsuarioNavigation { get; set; } = null!;

    public virtual Venta? Venta { get; set; }
}
