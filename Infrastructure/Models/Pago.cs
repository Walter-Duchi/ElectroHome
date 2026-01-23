using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Pago
{
    public int Id { get; set; }

    public int FkVenta { get; set; }

    public int FkMetodoPago { get; set; }

    public string? Estado { get; set; }

    public decimal Monto { get; set; }

    public string? Referencia { get; set; }

    public DateTime? FechaPago { get; set; }

    public string? DatosTransaccion { get; set; }

    public string? TerminalPuntoVenta { get; set; }

    public int? Cuotas { get; set; }

    public decimal? MontoCuota { get; set; }

    public virtual MetodosPago FkMetodoPagoNavigation { get; set; } = null!;

    public virtual Venta FkVentaNavigation { get; set; } = null!;
}
