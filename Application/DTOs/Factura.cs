using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public record Factura
    {
        public required string Marca;
        public required string Modelo;
        public required string NumSerie;
        public required DateTime VentaUsuario;
        public required int TiempoGarantia;
    }
}
