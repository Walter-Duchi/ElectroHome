using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public record VerFacturaDto
    {
        public required string Marca { get; init; }
        public required string Modelo { get; init; }
        public required string NumSerie { get; init; }
        public required DateTime VentaUsuario { get; init; }
        public required int TiempoGarantia { get; init; }
    }

}
