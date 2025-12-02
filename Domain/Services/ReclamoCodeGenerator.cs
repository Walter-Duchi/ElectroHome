using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services
{
    public class ReclamoCodeGenerator : IReclamoCodeGenerator
    {
        public string GenerarCodigo()
        {
            return $"REC-{DateTime.Now:yyyy}-{Guid.NewGuid().ToString().Substring(0,6).ToUpper()}";
        }
    }
}
