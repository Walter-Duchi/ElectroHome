using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ReembolsoPorReclamo
{
    public int Id { get; set; }

    public int FkReclamosProductoSn { get; set; }

    public int FkReembolso { get; set; }

    public virtual ReclamosProductoSn FkReclamosProductoSnNavigation { get; set; } = null!;

    public virtual Reembolso FkReembolsoNavigation { get; set; } = null!;
}
