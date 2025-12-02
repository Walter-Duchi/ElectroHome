using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public record ReclamoResponseDto
    (
        int Id,
        string CodigoReclamo,
        DateTime FechaVentaClienteFinal,
        DateTime FechaReclamoClienteFinal,
        string Estado
    );
}
