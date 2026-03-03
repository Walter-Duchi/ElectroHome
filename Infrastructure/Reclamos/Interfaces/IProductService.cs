using Application.DTOs.Ecommerce;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductResponse>> GetProductsAsync(ProductFilterRequest filter);
        Task<ProductResponse?> GetProductByIdAsync(int id);
        Task<List<ProductResponse>> GetPopularProductsAsync(int count = 10);
        Task<List<ProductResponse>> GetNewArrivalsAsync(int count = 10);
    }
}