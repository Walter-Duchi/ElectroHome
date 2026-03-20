using Application.DTOs.Inventario;

namespace Infrastructure.Reclamos.Interfaces;

public interface IInventoryService
{
    // Ubicaciones
    Task<List<UbicacionDto>> GetAllUbicacionesAsync();
    Task<UbicacionDto?> GetUbicacionByIdAsync(int id);
    Task<UbicacionDto> CreateUbicacionAsync(CreateUbicacionRequest request, int usuarioId);
    Task<UbicacionDto> UpdateUbicacionAsync(UpdateUbicacionRequest request, int usuarioId);
    Task<bool> DeleteUbicacionAsync(int id);

    // Movimientos
    Task<List<MovimientoInventarioDto>> GetMovimientosAsync(int? productoId = null, DateTime? desde = null, DateTime? hasta = null);
    Task<MovimientoInventarioDto> RegistrarEntradaAsync(CreateMovimientoRequest request, int usuarioId);
    Task<MovimientoInventarioDto> RegistrarSalidaAsync(CreateMovimientoRequest request, int usuarioId);
    Task<MovimientoInventarioDto> RegistrarAjusteAsync(CreateMovimientoRequest request, int usuarioId);
    Task<MovimientoInventarioDto> RegistrarDevolucionAsync(CreateMovimientoRequest request, int usuarioId);

    // Números de serie
    Task<List<NumeroSerieDto>> GetNumerosSerieAsync(int? productoId = null, string? estado = null, int? ubicacionId = null);
    Task<NumeroSerieDto?> GetNumeroSerieByNumeroAsync(string numeroSerie);
    Task<bool> UpdateNumeroSerieAsync(UpdateNumeroSerieRequest request, int usuarioId);

    // Proveedores
    Task<List<ProveedorDto>> GetAllProveedoresAsync(bool soloActivos = true);
    Task<ProveedorDto?> GetProveedorByIdAsync(int id);
    Task<ProveedorDto> CreateProveedorAsync(CreateProveedorRequest request, int usuarioId);
    Task<ProveedorDto> UpdateProveedorAsync(UpdateProveedorRequest request, int usuarioId);
    Task<bool> ToggleProveedorActivoAsync(int id, bool activo);
}