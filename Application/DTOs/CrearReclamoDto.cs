using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Application.DTOs
{
    public record CrearReclamoDto
    {
        public required string NumeroSerie { get; init; }
        public required string CodigoReclamo { get; init; }
        public DateTime? FechaVentaClienteFinal { get; init; }
        public required DateTime FechaReclamoClienteFinal {get; init;}
    }
}
