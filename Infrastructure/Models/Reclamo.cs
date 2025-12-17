namespace Infrastructure.Models;

public partial class Reclamo
{
    public int Id { get; set; }

    public string CodigoReclamo { get; set; } = null!;

    public int FkEmpresaCliente { get; set; }

    public DateTime? FechaCreacionReclamo { get; set; }

    public virtual Usuario FkEmpresaClienteNavigation { get; set; } = null!;

    public virtual ICollection<ReclamosProductoSn> ReclamosProductoSns { get; set; } = new List<ReclamosProductoSn>();
}
