namespace Infrastructure.Models;

public partial class Reembolso
{
    public int Id { get; set; }

    public string NumeroComprobanteReembolso { get; set; } = null!;

    public DateTime FechaReembolso { get; set; }

    public string NumCuentaBancariaReembolso { get; set; } = null!;

    public virtual ICollection<ReembolsoPorReclamo> ReembolsoPorReclamos { get; set; } = new List<ReembolsoPorReclamo>();
}
