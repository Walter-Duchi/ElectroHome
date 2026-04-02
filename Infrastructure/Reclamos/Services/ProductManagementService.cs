using Application.DTOs.Productos;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace Infrastructure.Reclamos.Services;

public class ProductManagementService : IProductManagementService
{
    private readonly ReclamosContext _context;
    private readonly ILogger<ProductManagementService> _logger;

    public ProductManagementService(ReclamosContext context, ILogger<ProductManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ========== Productos ==========
    public async Task<List<ProductoManagementDto>> GetAllProductosAsync(bool includeInactivos = false)
    {
        var query = _context.Productos
            .Include(p => p.FkMarcaNavigation)
            .Include(p => p.FkCategoriaNavigation)
            .Include(p => p.ProductoImagenes)
            .AsQueryable();

        if (!includeInactivos)
            query = query.Where(p => p.Activo == true);

        var productos = await query.OrderBy(p => p.Modelo).ToListAsync();
        return productos.Select(MapToProductoManagementDto).ToList();
    }

    public async Task<ProductoManagementDto?> GetProductoByIdAsync(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.FkMarcaNavigation)
            .Include(p => p.FkCategoriaNavigation)
            .Include(p => p.ProductoImagenes)
            .FirstOrDefaultAsync(p => p.Id == id);

        return producto == null ? null : MapToProductoManagementDto(producto);
    }

    public async Task<ProductoManagementDto> CreateProductoAsync(CreateProductoRequest request, int usuarioId, string webRootPath)
    {
        // Validar SKU único
        if (await _context.Productos.AnyAsync(p => p.Sku == request.Sku))
            throw new ArgumentException($"Ya existe un producto con SKU {request.Sku}");

        // Validar código único
        if (await _context.Productos.AnyAsync(p => p.Codigo == request.Codigo))
            throw new ArgumentException($"Ya existe un producto con código {request.Codigo}");

        // Validar marca y categoría
        var marca = await _context.Marcas.FindAsync(request.MarcaId);
        if (marca == null)
            throw new ArgumentException("Marca no válida");

        if (request.CategoriaId.HasValue)
        {
            var categoria = await _context.Categorias.FindAsync(request.CategoriaId);
            if (categoria == null)
                throw new ArgumentException("Categoría no válida");
        }

        var producto = new Producto
        {
            Sku = request.Sku,
            Codigo = request.Codigo,
            FkMarca = request.MarcaId,
            FkCategoria = request.CategoriaId,
            Modelo = request.Modelo,
            Especificacion = request.Especificacion,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            DiasGarantia = request.DiasGarantia,
            Visibilidad = request.Visibilidad,
            Activo = true,
            PesoKg = request.PesoKg,
            AltoCm = request.AltoCm,
            AnchoCm = request.AnchoCm,
            ProfundidadCm = request.ProfundidadCm,
            CreadoPor = usuarioId,
            FechaCreacion = DateTime.Now
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        // Guardar imagen principal
        if (request.ImagenPrincipal != null)
        {
            var imagenUrl = await GuardarImagen(request.ImagenPrincipal, producto.Id, true);
            producto.ImagenUrl = imagenUrl;
        }

        // Guardar imágenes adicionales
        if (request.ImagenesAdicionales != null)
        {
            foreach (var img in request.ImagenesAdicionales)
            {
                var url = await GuardarImagen(img, producto.Id, false);
                _context.ProductoImagenes.Add(new ProductoImagene
                {
                    FkProducto = producto.Id,
                    UrlImagen = url,
                    EsPrincipal = false
                });
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Usuario {UsuarioId} creó producto {Sku} - {Modelo}", usuarioId, request.Sku, request.Modelo);

        return await GetProductoByIdAsync(producto.Id) ?? throw new Exception("Error al recuperar producto creado");
    }

    public async Task<ProductoManagementDto> UpdateProductoAsync(UpdateProductoRequest request, int usuarioId, string webRootPath)
    {
        var producto = await _context.Productos
            .Include(p => p.ProductoImagenes)
            .FirstOrDefaultAsync(p => p.Id == request.Id);

        if (producto == null)
            throw new ArgumentException("Producto no encontrado");

        // Validar SKU único si cambió
        if (producto.Sku != request.Sku &&
            await _context.Productos.AnyAsync(p => p.Sku == request.Sku && p.Id != request.Id))
            throw new ArgumentException($"Ya existe otro producto con SKU {request.Sku}");

        // Validar código único si cambió
        if (producto.Codigo != request.Codigo &&
            await _context.Productos.AnyAsync(p => p.Codigo == request.Codigo && p.Id != request.Id))
            throw new ArgumentException($"Ya existe otro producto con código {request.Codigo}");

        producto.Sku = request.Sku;
        producto.Codigo = request.Codigo;
        producto.FkMarca = request.MarcaId;
        producto.FkCategoria = request.CategoriaId;
        producto.Modelo = request.Modelo;
        producto.Especificacion = request.Especificacion;
        producto.Descripcion = request.Descripcion;
        producto.Precio = request.Precio;
        producto.DiasGarantia = request.DiasGarantia;
        producto.Visibilidad = request.Visibilidad;
        producto.Activo = request.Activo;
        producto.PesoKg = request.PesoKg;
        producto.AltoCm = request.AltoCm;
        producto.AnchoCm = request.AnchoCm;
        producto.ProfundidadCm = request.ProfundidadCm;
        producto.ModificadoPor = usuarioId;

        // Actualizar imagen principal
        if (request.ImagenPrincipal != null)
        {
            if (!string.IsNullOrEmpty(producto.ImagenUrl))
                EliminarArchivo(producto.ImagenUrl);

            producto.ImagenUrl = await GuardarImagen(request.ImagenPrincipal, producto.Id, true);
        }

        // Eliminar imágenes solicitadas
        if (request.ImagenesAEliminar != null)
        {
            var imagenesAEliminar = producto.ProductoImagenes
                .Where(pi => request.ImagenesAEliminar.Contains(pi.UrlImagen))
                .ToList();

            foreach (var img in imagenesAEliminar)
            {
                EliminarArchivo(img.UrlImagen);
                _context.ProductoImagenes.Remove(img);
            }
        }

        // Agregar nuevas imágenes adicionales
        if (request.ImagenesAdicionales != null)
        {
            foreach (var img in request.ImagenesAdicionales)
            {
                var url = await GuardarImagen(img, producto.Id, false);
                _context.ProductoImagenes.Add(new ProductoImagene
                {
                    FkProducto = producto.Id,
                    UrlImagen = url,
                    EsPrincipal = false
                });
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Usuario {UsuarioId} actualizó producto {Id}", usuarioId, request.Id);

        return await GetProductoByIdAsync(producto.Id) ?? throw new Exception("Error al recuperar producto actualizado");
    }

    public async Task<bool> ToggleProductoActivoAsync(int id, bool activo, int usuarioId)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
            return false;

        producto.Activo = activo;
        producto.ModificadoPor = usuarioId;
        await _context.SaveChangesAsync();

        string accion = activo ? "activó" : "desactivó";
        _logger.LogInformation("Usuario {UsuarioId} {Accion} producto {Id}", usuarioId, accion, id);
        return true;
    }

    // ========== Categorías ==========
    public async Task<List<CategoriaDto>> GetAllCategoriasAsync(bool includeInactivos = false)
    {
        var query = _context.Categorias
            .Include(c => c.FkCategoriaPadreNavigation)
            .AsQueryable();

        if (!includeInactivos)
            query = query.Where(c => c.Activo == true);

        var categorias = await query.OrderBy(c => c.Nombre).ToListAsync();
        return categorias.Select(MapToCategoriaDto).ToList();
    }

    public async Task<CategoriaDto?> GetCategoriaByIdAsync(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.FkCategoriaPadreNavigation)
            .FirstOrDefaultAsync(c => c.Id == id);

        return categoria == null ? null : MapToCategoriaDto(categoria);
    }

    public async Task<CategoriaDto> CreateCategoriaAsync(CreateCategoriaRequest request, int usuarioId)
    {
        if (await _context.Categorias.AnyAsync(c => c.Nombre == request.Nombre))
            throw new ArgumentException($"Ya existe una categoría con el nombre {request.Nombre}");

        var categoria = new Categoria
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Activo = request.Activo,
            FkCategoriaPadre = request.CategoriaPadreId,
            FechaCreacion = DateTime.Now
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UsuarioId} creó categoría {Nombre}", usuarioId, request.Nombre);
        return MapToCategoriaDto(categoria);
    }

    public async Task<CategoriaDto> UpdateCategoriaAsync(UpdateCategoriaRequest request, int usuarioId)
    {
        var categoria = await _context.Categorias.FindAsync(request.Id);
        if (categoria == null)
            throw new ArgumentException("Categoría no encontrada");

        if (categoria.Nombre != request.Nombre &&
            await _context.Categorias.AnyAsync(c => c.Nombre == request.Nombre && c.Id != request.Id))
            throw new ArgumentException($"Ya existe otra categoría con el nombre {request.Nombre}");

        categoria.Nombre = request.Nombre;
        categoria.Descripcion = request.Descripcion;
        categoria.Activo = request.Activo;
        categoria.FkCategoriaPadre = request.CategoriaPadreId;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Usuario {UsuarioId} actualizó categoría {Id}", usuarioId, request.Id);

        return MapToCategoriaDto(categoria);
    }

    public async Task<bool> DeleteCategoriaAsync(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.InverseFkCategoriaPadreNavigation)
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
            return false;

        if (categoria.InverseFkCategoriaPadreNavigation.Any() || categoria.Productos.Any())
            throw new InvalidOperationException("No se puede eliminar una categoría que tiene subcategorías o productos asociados.");

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return true;
    }

    // ========== Marcas ==========
    public async Task<List<MarcaDto>> GetAllMarcasAsync()
    {
        var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
        return marcas.Select(MapToMarcaDto).ToList();
    }

    public async Task<MarcaDto?> GetMarcaByIdAsync(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        return marca == null ? null : MapToMarcaDto(marca);
    }

    public async Task<MarcaDto> CreateMarcaAsync(CreateMarcaRequest request, int usuarioId)
    {
        if (await _context.Marcas.AnyAsync(m => m.Nombre == request.Nombre))
            throw new ArgumentException($"Ya existe una marca con el nombre {request.Nombre}");

        var marca = new Marca { Nombre = request.Nombre };
        _context.Marcas.Add(marca);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UsuarioId} creó marca {Nombre}", usuarioId, request.Nombre);
        return MapToMarcaDto(marca);
    }

    public async Task<MarcaDto> UpdateMarcaAsync(UpdateMarcaRequest request, int usuarioId)
    {
        var marca = await _context.Marcas.FindAsync(request.Id);
        if (marca == null)
            throw new ArgumentException("Marca no encontrada");

        if (marca.Nombre != request.Nombre &&
            await _context.Marcas.AnyAsync(m => m.Nombre == request.Nombre && m.Id != request.Id))
            throw new ArgumentException($"Ya existe otra marca con el nombre {request.Nombre}");

        marca.Nombre = request.Nombre;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UsuarioId} actualizó marca {Id}", usuarioId, request.Id);
        return MapToMarcaDto(marca);
    }

    public async Task<bool> DeleteMarcaAsync(int id)
    {
        var marca = await _context.Marcas
            .Include(m => m.Productos)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (marca == null)
            return false;

        if (marca.Productos.Any())
            throw new InvalidOperationException("No se puede eliminar una marca que tiene productos asociados.");

        _context.Marcas.Remove(marca);
        await _context.SaveChangesAsync();
        return true;
    }

    // ========== Métodos auxiliares ==========
    private string GetFrontendImagesPath()
    {
        var baseDir = Directory.GetCurrentDirectory();
        var frontendImagesPath = Path.GetFullPath(Path.Combine(baseDir, "..", "Frontend", "public", "img", "products"));
        return frontendImagesPath;
    }

    private async Task<string> GuardarImagen(IFormFile archivo, int productoId, bool esPrincipal = true)
    {
        var extension = Path.GetExtension(archivo.FileName);
        var nombreArchivo = esPrincipal
            ? $"prod_{productoId}_principal{extension}"
            : $"prod_{productoId}_{Guid.NewGuid():N}{extension}";

        var relativeUrl = $"/img/products/{nombreArchivo}";
        var imagesPath = GetFrontendImagesPath();
        var fullPath = Path.Combine(imagesPath, nombreArchivo);

        var directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        return relativeUrl;
    }

    private void EliminarArchivo(string urlImagen)
    {
        if (string.IsNullOrEmpty(urlImagen))
            return;

        var nombreArchivo = Path.GetFileName(urlImagen);
        var imagesPath = GetFrontendImagesPath();
        var fullPath = Path.Combine(imagesPath, nombreArchivo);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    private ProductoManagementDto MapToProductoManagementDto(Producto p)
    {
        return new ProductoManagementDto
        {
            Id = p.Id,
            Sku = p.Sku,
            Codigo = p.Codigo,
            Modelo = p.Modelo,
            MarcaId = p.FkMarca,
            MarcaNombre = p.FkMarcaNavigation?.Nombre ?? "",
            CategoriaId = p.FkCategoria,
            CategoriaNombre = p.FkCategoriaNavigation?.Nombre,
            Especificacion = p.Especificacion,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            DiasGarantia = p.DiasGarantia,
            Visibilidad = p.Visibilidad,
            Activo = p.Activo ?? true,
            PesoKg = p.PesoKg,
            AltoCm = p.AltoCm,
            AnchoCm = p.AnchoCm,
            ProfundidadCm = p.ProfundidadCm,
            ImagenUrl = p.ImagenUrl,
            ImagenesAdicionales = p.ProductoImagenes?.Where(pi => pi.EsPrincipal == false).Select(pi => pi.UrlImagen).ToList() ?? new(),
            FechaCreacion = p.FechaCreacion ?? DateTime.Now,
            CreadoPor = p.CreadoPor,
            ModificadoPor = p.ModificadoPor
        };
    }

    private CategoriaDto MapToCategoriaDto(Categoria c)
    {
        return new CategoriaDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion,
            Activo = c.Activo ?? true,
            CategoriaPadreId = c.FkCategoriaPadre,
            CategoriaPadreNombre = c.FkCategoriaPadreNavigation?.Nombre,
            FechaCreacion = c.FechaCreacion ?? DateTime.Now
        };
    }

    private MarcaDto MapToMarcaDto(Marca m)
    {
        return new MarcaDto { Id = m.Id, Nombre = m.Nombre };
    }
}