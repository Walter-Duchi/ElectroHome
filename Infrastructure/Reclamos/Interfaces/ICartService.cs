using Application.DTOs.Ecommerce;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItemResponse>> GetCartAsync(int usuarioId);
        Task<bool> AddToCartAsync(int usuarioId, AddToCartRequest request);
        Task<bool> UpdateCartItemQuantityAsync(int usuarioId, int productoId, int nuevaCantidad);
        Task<bool> RemoveFromCartAsync(int usuarioId, int productoId);
        Task<bool> ClearCartAsync(int usuarioId);
        Task RemoveProductFromAllCartsAsync(int productoId);
    }
}