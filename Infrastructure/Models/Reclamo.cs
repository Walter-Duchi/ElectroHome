using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Reclamo
{
    public int Id { get; set; }

    public string CodigoReclamo { get; set; } = null!;

    public int FkDetalleCompra { get; set; }

    public int FkClienteFinal { get; set; }

    public DateTime FechaVentaClienteFinal { get; set; }

    public DateTime FechaReclamoClienteFinal { get; set; }

    public DateTime? FechaCreacionReclamo { get; set; }

    public string? Estado { get; set; }

    public int? FkTecnicoAsignado { get; set; }

    public string? FormaCompensacion { get; set; }

    public DateTime? FechaRespuesta { get; set; }

    public string? ExplicacionRespuesta { get; set; }

    public string? PdfEvidenciaRevision { get; set; }

    public DateTime? FechaNotificacionLeida { get; set; }

    public DateTime? FechaReembolso { get; set; }

    public string? NumeroComprobanteReembolso { get; set; }

    public string? PdfComprobanteEntregaCliente { get; set; }

    public virtual Usuario FkClienteFinalNavigation { get; set; } = null!;

    public virtual DetallesCompra FkDetalleCompraNavigation { get; set; } = null!;

    public virtual Usuario? FkTecnicoAsignadoNavigation { get; set; }
}
