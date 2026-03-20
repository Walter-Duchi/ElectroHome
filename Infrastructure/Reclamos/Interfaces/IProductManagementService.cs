using Application.DTOs.Productos;

namespace Infrastructure.Reclamos.Interfaces;

public interface IProductManagementService
{
    // Productos
    Task<List<ProductoManagementDto>> GetAllProductosAsync(bool includeInactivos = false);
    Task<ProductoManagementDto?> GetProductoByIdAsync(int id);
    Task<ProductoManagementDto> CreateProductoAsync(CreateProductoRequest request, int usuarioId, string webRootPath);
    Task<ProductoManagementDto> UpdateProductoAsync(UpdateProductoRequest request, int usuarioId, string webRootPath);
    Task<bool> ToggleProductoActivoAsync(int id, bool activo, int usuarioId);

    // Categorías
    Task<List<CategoriaDto>> GetAllCategoriasAsync(bool includeInactivos = false);
    Task<CategoriaDto?> GetCategoriaByIdAsync(int id);
    Task<CategoriaDto> CreateCategoriaAsync(CreateCategoriaRequest request, int usuarioId);
    Task<CategoriaDto> UpdateCategoriaAsync(UpdateCategoriaRequest request, int usuarioId);
    Task<bool> DeleteCategoriaAsync(int id);

    // Marcas
    Task<List<MarcaDto>> GetAllMarcasAsync();
    Task<MarcaDto?> GetMarcaByIdAsync(int id);
    Task<MarcaDto> CreateMarcaAsync(CreateMarcaRequest request, int usuarioId);
    Task<MarcaDto> UpdateMarcaAsync(UpdateMarcaRequest request, int usuarioId);
    Task<bool> DeleteMarcaAsync(int id);
}