using Application.DTOs.Ecommerce;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetAllCategoriesAsync();
        Task<CategoryResponse?> GetCategoryByIdAsync(int id);
    }
}