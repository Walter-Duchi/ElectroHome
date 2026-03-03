using Application.DTOs.Ecommerce;
using Infrastructure.Data;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reclamos.Services
{
    public class ProductService : IProductService
    {
        private readonly ReclamosContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ReclamosContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ProductResponse>> GetProductsAsync(ProductFilterRequest filter)
        {
            try
            {
                var query = _context.Productos
                    .Include(p => p.FkMarcaNavigation)
                    .Include(p => p.FkCategoriaNavigation)
                    .Include(p => p.ProductoImagenes)
                    .Include(p => p.NumeroSerieProductos) // necesario para contar stock
                    .Where(p => p.Activo == true && p.Visibilidad == "Publico")
                    // Solo productos con al menos una unidad vendible
                    .Where(p => p.NumeroSerieProductos.Any(nsp => nsp.EstadoInventario == "Se_Puede_Vender"));

                if (!string.IsNullOrWhiteSpace(filter.Busqueda))
                {
                    var busqueda = filter.Busqueda.ToLower();
                    query = query.Where(p =>
                        p.Modelo.ToLower().Contains(busqueda) ||
                        (p.Descripcion != null && p.Descripcion.ToLower().Contains(busqueda)) ||
                        p.FkMarcaNavigation.Nombre.ToLower().Contains(busqueda));
                }

                if (filter.CategoriaId.HasValue)
                {
                    query = query.Where(p => p.FkCategoria == filter.CategoriaId.Value);
                }

                if (filter.PrecioMin.HasValue)
                {
                    query = query.Where(p => p.Precio >= filter.PrecioMin.Value);
                }

                if (filter.PrecioMax.HasValue)
                {
                    query = query.Where(p => p.Precio <= filter.PrecioMax.Value);
                }

                // Ordenamiento
                query = filter.OrdenarPor switch
                {
                    "precio_asc" => query.OrderBy(p => p.Precio),
                    "precio_desc" => query.OrderByDescending(p => p.Precio),
                    "nuevos" => query.OrderByDescending(p => p.FechaCreacion),
                    _ => query.OrderBy(p => p.Modelo) // por defecto alfabético
                };

                var productos = await query
                    .Select(p => new ProductResponse
                    {
                        Id = p.Id,
                        SKU = p.Sku,
                        Nombre = $"{p.FkMarcaNavigation.Nombre} {p.Modelo}",
                        Marca = p.FkMarcaNavigation.Nombre,
                        Categoria = p.FkCategoriaNavigation != null ? p.FkCategoriaNavigation.Nombre : null,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        ImagenPrincipal = p.ProductoImagenes.FirstOrDefault(pi => (bool)pi.EsPrincipal) != null
                            ? p.ProductoImagenes.First(pi => (bool)pi.EsPrincipal).UrlImagen
                            : p.ImagenUrl,
                        ImagenesAdicionales = p.ProductoImagenes.Where(pi => (bool)!pi.EsPrincipal).Select(pi => pi.UrlImagen).ToList(),
                        StockDisponible = p.NumeroSerieProductos.Count(nsp => nsp.EstadoInventario == "Se_Puede_Vender"),
                        DiasGarantia = p.DiasGarantia,
                        Activo = (bool)p.Activo
                    })
                    .ToListAsync();

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                throw;
            }
        }

        public async Task<ProductResponse?> GetProductByIdAsync(int id)
        {
            try
            {
                var producto = await _context.Productos
                    .Include(p => p.FkMarcaNavigation)
                    .Include(p => p.FkCategoriaNavigation)
                    .Include(p => p.ProductoImagenes)
                    .Include(p => p.NumeroSerieProductos)
                    .Where(p => p.Id == id && p.Activo == true)
                    .Select(p => new ProductResponse
                    {
                        Id = p.Id,
                        SKU = p.Sku,
                        Nombre = $"{p.FkMarcaNavigation.Nombre} {p.Modelo}",
                        Marca = p.FkMarcaNavigation.Nombre,
                        Categoria = p.FkCategoriaNavigation != null ? p.FkCategoriaNavigation.Nombre : null,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        ImagenPrincipal = p.ProductoImagenes.FirstOrDefault(pi => (bool)pi.EsPrincipal) != null
                            ? p.ProductoImagenes.First(pi => (bool)pi.EsPrincipal).UrlImagen
                            : p.ImagenUrl,
                        ImagenesAdicionales = p.ProductoImagenes.Where(pi => !(bool)pi.EsPrincipal).Select(pi => pi.UrlImagen).ToList(),
                        StockDisponible = p.NumeroSerieProductos.Count(nsp => nsp.EstadoInventario == "Se_Puede_Vender"),
                        DiasGarantia = p.DiasGarantia,
                        Activo = (bool)p.Activo
                    })
                    .FirstOrDefaultAsync();

                return producto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto por ID {Id}", id);
                throw;
            }
        }

        public async Task<List<ProductResponse>> GetPopularProductsAsync(int count = 10)
        {
            // Simulación: devolvemos los productos con más stock vendido (o más recientes)
            // En un caso real podrías consultar ventas o una tabla de productos populares
            var productos = await GetProductsAsync(new ProductFilterRequest());
            return productos.OrderByDescending(p => p.StockDisponible).Take(count).ToList();
        }

        public async Task<List<ProductResponse>> GetNewArrivalsAsync(int count = 10)
        {
            var productos = await _context.Productos
                .Include(p => p.FkMarcaNavigation)
                .Include(p => p.ProductoImagenes)
                .Include(p => p.NumeroSerieProductos)
                .Where(p => p.Activo == true && p.Visibilidad == "Publico")
                .Where(p => p.NumeroSerieProductos.Any(nsp => nsp.EstadoInventario == "Se_Puede_Vender"))
                .OrderByDescending(p => p.FechaCreacion)
                .Take(count)
                .Select(p => new ProductResponse
                {
                    Id = p.Id,
                    SKU = p.Sku,
                    Nombre = $"{p.FkMarcaNavigation.Nombre} {p.Modelo}",
                    Marca = p.FkMarcaNavigation.Nombre,
                    Precio = p.Precio,
                    ImagenPrincipal = p.ProductoImagenes.FirstOrDefault(pi => (bool)pi.EsPrincipal) != null
                            ? p.ProductoImagenes.First(pi => (bool)pi.EsPrincipal).UrlImagen
                            : p.ImagenUrl,
                    StockDisponible = p.NumeroSerieProductos.Count(nsp => nsp.EstadoInventario == "Se_Puede_Vender")
                })
                .ToListAsync();

            return productos;
        }
    }
}