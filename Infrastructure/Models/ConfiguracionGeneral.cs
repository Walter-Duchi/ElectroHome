namespace Infrastructure.Models;

public partial class ConfiguracionGeneral
{
    public int Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Valor { get; set; } = null!;

    public string? Tipo { get; set; }

    public string? Categoria { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? ModificadoPor { get; set; }

    public virtual Usuario? ModificadoPorNavigation { get; set; }
}
