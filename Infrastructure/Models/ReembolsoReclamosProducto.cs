using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ReembolsoReclamosProducto
{
    public int Id { get; set; }

    public int FkReclamosProductos { get; set; }

    public int FkReembolso { get; set; }

    public virtual ReclamosProducto FkReclamosProductosNavigation { get; set; } = null!;

    public virtual Reembolso FkReembolsoNavigation { get; set; } = null!;
}
