using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ComprobanteProductoReemplazado
{
    public int Id { get; set; }

    public int FkReclamosProductoSn { get; set; }

    public int FkComprobanteDeReemplazo { get; set; }

    public int FkProductoDeReemplazo { get; set; }

    public virtual ComprobanteDeReemplazo FkComprobanteDeReemplazoNavigation { get; set; } = null!;

    public virtual NumeroSerieProducto FkProductoDeReemplazoNavigation { get; set; } = null!;

    public virtual ReclamosProductoSn FkReclamosProductoSnNavigation { get; set; } = null!;
}
