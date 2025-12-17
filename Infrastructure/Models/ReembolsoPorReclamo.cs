namespace Infrastructure.Models;

public partial class ReembolsoPorReclamo
{
    public int Id { get; set; }

    public int ReclamosProductoSn { get; set; }

    public int FkReembolso { get; set; }

    public virtual Reembolso FkReembolsoNavigation { get; set; } = null!;

    public virtual ReclamosProductoSn ReclamosProductoSnNavigation { get; set; } = null!;
}
