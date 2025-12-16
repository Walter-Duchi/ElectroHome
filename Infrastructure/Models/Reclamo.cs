using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Reclamo
{
    public int Id { get; set; }

    public string CodigoReclamo { get; set; } = null!;

    public int EmpresaCliente { get; set; }

    public DateTime? FechaCreacionReclamo { get; set; }

    public virtual Usuario EmpresaClienteNavigation { get; set; } = null!;

    public virtual ReclamosProducto? ReclamosProducto { get; set; }
}
