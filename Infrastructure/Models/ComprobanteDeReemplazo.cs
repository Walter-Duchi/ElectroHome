using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ComprobanteDeReemplazo
{
    public int Id { get; set; }

    public string PdfComprobanteEntregaCliente { get; set; } = null!;

    public int FkPersonalEntrega { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<ComprobanteProductoReemplazado> ComprobanteProductoReemplazados { get; set; } = new List<ComprobanteProductoReemplazado>();

    public virtual Usuario FkPersonalEntregaNavigation { get; set; } = null!;
}
