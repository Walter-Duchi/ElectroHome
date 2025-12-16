using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class EvidenciaReemplazoReclamosProducto
{
    public int Id { get; set; }

    public int FkReclamosProductos { get; set; }

    public int FkEvidenciaReemplazo { get; set; }

    public virtual EvidenciaReemplazo FkEvidenciaReemplazoNavigation { get; set; } = null!;

    public virtual ReclamosProducto FkReclamosProductosNavigation { get; set; } = null!;
}
