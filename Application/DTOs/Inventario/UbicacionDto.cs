namespace Application.DTOs.Inventario;

public class UbicacionDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Tipo { get; set; }
    public int? UbicacionPadreId { get; set; }
    public string? UbicacionPadreNombre { get; set; }
    public int? CapacidadMaxima { get; set; }
    public bool Activo { get; set; }
}

public class CreateUbicacionRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Tipo { get; set; }
    public int? UbicacionPadreId { get; set; }
    public int? CapacidadMaxima { get; set; }
    public bool Activo { get; set; } = true;
}

public class UpdateUbicacionRequest
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Tipo { get; set; }
    public int? UbicacionPadreId { get; set; }
    public int? CapacidadMaxima { get; set; }
    public bool Activo { get; set; }
}