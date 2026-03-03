using Application.DTOs.Ecommerce;
using Infrastructure.Data;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reclamos.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ReclamosContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ReclamosContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CategoryResponse>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categorias
                    .Where(c => c.Activo == true)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new CategoryResponse
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        CategoriaPadreId = c.FkCategoriaPadre
                    })
                    .ToListAsync();

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                throw;
            }
        }

        public async Task<CategoryResponse?> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.Categorias
                    .Where(c => c.Id == id && c.Activo == true)
                    .Select(c => new CategoryResponse
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        CategoriaPadreId = c.FkCategoriaPadre
                    })
                    .FirstOrDefaultAsync();

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría por ID {Id}", id);
                throw;
            }
        }
    }
}