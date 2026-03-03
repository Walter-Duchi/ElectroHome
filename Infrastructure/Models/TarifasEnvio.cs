namespace Infrastructure.Models;

public partial class TarifasEnvio
{
    public int Id { get; set; }

    public string Zona { get; set; } = null!;

    public decimal? PesoMinimo { get; set; }

    public decimal? PesoMaximo { get; set; }

    public decimal Precio { get; set; }

    public int TiempoEntregaDias { get; set; }

    public bool? Activo { get; set; }

    public int? FkTransportista { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}
