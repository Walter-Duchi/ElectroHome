namespace Application.DTOs.Inventario;

public class MovimientoInventarioDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSku { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string TipoMovimiento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int CantidadAnterior { get; set; }
    public int CantidadNueva { get; set; }
    public string? Motivo { get; set; }
    public string? Referencia { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public decimal? CostoUnitario { get; set; }
}

public class CreateMovimientoRequest
{
    public int ProductoId { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty; // Entrada, Salida, Ajuste, Devolucion
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public string? Referencia { get; set; }
    public decimal? CostoUnitario { get; set; }
    // Para entradas masivas con números de serie
    public List<CreateNumeroSerieRequest>? NumerosSerie { get; set; }
}

public class CreateNumeroSerieRequest
{
    public string NumeroSerie { get; set; } = string.Empty;
    public int ProveedorId { get; set; }
    public int? UbicacionId { get; set; }
}