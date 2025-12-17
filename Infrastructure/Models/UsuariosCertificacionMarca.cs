namespace Infrastructure.Models;

public partial class UsuariosCertificacionMarca
{
    public int Id { get; set; }

    public int FkMarca { get; set; }

    public int FkTecnico { get; set; }

    public virtual Marca FkMarcaNavigation { get; set; } = null!;

    public virtual Usuario FkTecnicoNavigation { get; set; } = null!;
}
