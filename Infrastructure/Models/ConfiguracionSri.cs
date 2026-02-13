using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ConfiguracionSri
{
    public int Id { get; set; }

    public string? Ambiente { get; set; }

    public string? TokenAcceso { get; set; }

    public DateTime? FechaExpiracionToken { get; set; }
}
