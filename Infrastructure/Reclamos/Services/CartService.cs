using Application.DTOs.Ecommerce;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reclamos.Services
{
    public class CartService : ICartService
    {
        private readonly ReclamosContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(ReclamosContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CartItemResponse>> GetCartAsync(int usuarioId)
        {
            try
            {
                var items = await _context.CarritoCompras
                    .Include(c => c.FkProductoNavigation)
                        .ThenInclude(p => p.FkMarcaNavigation)
                    .Include(c => c.FkProductoNavigation)
                        .ThenInclude(p => p.ProductoImagenes)
                    .Where(c => c.FkCliente == usuarioId)
                    .OrderByDescending(c => c.FechaAgregado)
                    .Select(c => new CartItemResponse
                    {
                        Id = c.Id,
                        ProductoId = c.FkProducto,
                        NombreProducto = c.FkProductoNavigation.FkMarcaNavigation.Nombre + " " + c.FkProductoNavigation.Modelo,
                        ImagenUrl = c.FkProductoNavigation.ProductoImagenes.FirstOrDefault(pi => (bool)pi.EsPrincipal) != null
                            ? c.FkProductoNavigation.ProductoImagenes.First(pi => (bool)pi.EsPrincipal).UrlImagen
                            : c.FkProductoNavigation.ImagenUrl,
                        PrecioUnitario = c.FkProductoNavigation.Precio,
                        Cantidad = c.Cantidad,
                        FechaAgregado = (DateTime)c.FechaAgregado
                    })
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener carrito del usuario {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<bool> AddToCartAsync(int usuarioId, AddToCartRequest request)
        {
            try
            {
                // Verificar que el producto existe y está activo
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == request.ProductoId && p.Activo == true);
                if (producto == null)
                    return false;

                // Verificar stock disponible
                var stock = await _context.NumeroSerieProductos
                    .CountAsync(nsp => nsp.FkProducto == request.ProductoId && nsp.EstadoInventario == "Se_Puede_Vender");
                if (stock < request.Cantidad)
                    return false;

                // Buscar si ya existe el producto en el carrito
                var existingItem = await _context.CarritoCompras
                    .FirstOrDefaultAsync(c => c.FkCliente == usuarioId && c.FkProducto == request.ProductoId);

                if (existingItem != null)
                {
                    // Actualizar cantidad
                    existingItem.Cantidad += request.Cantidad;
                }
                else
                {
                    // Crear nuevo item
                    var newItem = new CarritoCompra
                    {
                        FkCliente = usuarioId,
                        FkProducto = request.ProductoId,
                        Cantidad = request.Cantidad,
                        FechaAgregado = DateTime.UtcNow
                    };
                    _context.CarritoCompras.Add(newItem);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar producto al carrito para usuario {UsuarioId}", usuarioId);
                return false;
            }
        }

        public async Task<bool> UpdateCartItemQuantityAsync(int usuarioId, int productoId, int nuevaCantidad)
        {
            try
            {
                var item = await _context.CarritoCompras
                    .FirstOrDefaultAsync(c => c.FkCliente == usuarioId && c.FkProducto == productoId);
                if (item == null)
                    return false;

                if (nuevaCantidad <= 0)
                {
                    _context.CarritoCompras.Remove(item);
                }
                else
                {
                    // Verificar stock
                    var stock = await _context.NumeroSerieProductos
                        .CountAsync(nsp => nsp.FkProducto == productoId && nsp.EstadoInventario == "Se_Puede_Vender");
                    if (stock < nuevaCantidad)
                        return false;

                    item.Cantidad = nuevaCantidad;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cantidad del carrito para usuario {UsuarioId}", usuarioId);
                return false;
            }
        }

        public async Task<bool> RemoveFromCartAsync(int usuarioId, int productoId)
        {
            try
            {
                var item = await _context.CarritoCompras
                    .FirstOrDefaultAsync(c => c.FkCliente == usuarioId && c.FkProducto == productoId);
                if (item == null)
                    return false;

                _context.CarritoCompras.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto del carrito para usuario {UsuarioId}", usuarioId);
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(int usuarioId)
        {
            try
            {
                var items = await _context.CarritoCompras
                    .Where(c => c.FkCliente == usuarioId)
                    .ToListAsync();
                _context.CarritoCompras.RemoveRange(items);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar carrito del usuario {UsuarioId}", usuarioId);
                return false;
            }
        }
    }
}