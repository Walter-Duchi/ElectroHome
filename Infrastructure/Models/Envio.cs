using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Envio
{
    public int Id { get; set; }

    public int FkVenta { get; set; }

    public string? Transportista { get; set; }

    public string? GuiaRemision { get; set; }

    public decimal? CostoEnvio { get; set; }

    public decimal? PesoTotal { get; set; }

    public string? DimensionesTotal { get; set; }

    public string? EstadoEnvio { get; set; }

    public DateTime? FechaDespacho { get; set; }

    public DateTime? FechaEstimadaEntrega { get; set; }

    public DateTime? FechaRealEntrega { get; set; }

    public string? FirmadoPor { get; set; }

    public string? EvidenciaEntrega { get; set; }

    public virtual Venta FkVentaNavigation { get; set; } = null!;
}
