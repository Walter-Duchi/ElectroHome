using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public record ReclamoRevisor
    {
        public required int CodigoReclamo { get; init; }
    }
}
