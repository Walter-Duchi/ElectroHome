using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ConfiguracionSri
{
    public int Id { get; set; }

    public string? Ambiente { get; set; }

    public string Establecimiento { get; set; } = null!;

    public string PuntoEmision { get; set; } = null!;

    public int SecuencialFactura { get; set; }

    public int SecuencialNotaCredito { get; set; }

    public string RucEmpresa { get; set; } = null!;

    public string NombreComercial { get; set; } = null!;

    public string RazonSocial { get; set; } = null!;

    public string DireccionMatriz { get; set; } = null!;

    public bool? ObligadoContabilidad { get; set; }

    public string? TokenAcceso { get; set; }

    public DateTime? FechaExpiracionToken { get; set; }
}
