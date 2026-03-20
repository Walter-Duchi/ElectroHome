namespace Application.DTOs.Inventario;

public class NumeroSerieDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string EstadoInventario { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public int? UbicacionId { get; set; }
    public string? UbicacionNombre { get; set; }
}

public class UpdateNumeroSerieRequest
{
    public int Id { get; set; }
    public int? UbicacionId { get; set; }
    public string EstadoInventario { get; set; } = string.Empty;
}