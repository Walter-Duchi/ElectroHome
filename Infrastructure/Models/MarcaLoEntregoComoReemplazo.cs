using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class MarcaLoEntregoComoReemplazo
{
    public int Id { get; set; }

    public int FkNumeroSerieProductos { get; set; }

    public virtual NumeroSerieProducto FkNumeroSerieProductosNavigation { get; set; } = null!;
}
