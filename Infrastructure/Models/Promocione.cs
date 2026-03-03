namespace Infrastructure.Models;

public partial class Promocione
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Tipo { get; set; }

    public decimal? Valor { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public bool? Activo { get; set; }
}
