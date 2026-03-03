namespace Infrastructure.Models;

public partial class DatosEmpresa
{
    public int Id { get; set; }

    public string RucEmpresa { get; set; } = null!;

    public string NombreComercial { get; set; } = null!;

    public string RazonSocial { get; set; } = null!;

    public string DireccionMatriz { get; set; } = null!;
}
