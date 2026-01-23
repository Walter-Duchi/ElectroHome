using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Reembolso
{
    public int Id { get; set; }

    public string NumeroComprobanteReembolso { get; set; } = null!;

    public DateTime FechaReembolso { get; set; }

    public string NumCuentaBancariaReembolso { get; set; } = null!;

    public int? FkMetodoPago { get; set; }

    public string? Estado { get; set; }

    public string? ReferenciaBancaria { get; set; }

    public string? ComprobantePago { get; set; }

    public int? FkUsuarioAutorizo { get; set; }

    public DateTime? FechaAutorizacion { get; set; }

    public virtual MetodosPago? FkMetodoPagoNavigation { get; set; }

    public virtual Usuario? FkUsuarioAutorizoNavigation { get; set; }

    public virtual ICollection<ReembolsoPorReclamo> ReembolsoPorReclamos { get; set; } = new List<ReembolsoPorReclamo>();
}
