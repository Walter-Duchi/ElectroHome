using Application.DTOs.Inventario;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reclamos.Services;

public class InventoryService : IInventoryService
{
    private readonly ReclamosContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(ReclamosContext context, ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ========== Ubicaciones ==========
    public async Task<List<UbicacionDto>> GetAllUbicacionesAsync()
    {
        var ubicaciones = await _context.InventarioUbicaciones
            .Include(u => u.FkUbicacionPadreNavigation)
            .OrderBy(u => u.Codigo)
            .ToListAsync();

        return ubicaciones.Select(MapToUbicacionDto).ToList();
    }

    public async Task<UbicacionDto?> GetUbicacionByIdAsync(int id)
    {
        var ubicacion = await _context.InventarioUbicaciones
            .Include(u => u.FkUbicacionPadreNavigation)
            .FirstOrDefaultAsync(u => u.Id == id);

        return ubicacion == null ? null : MapToUbicacionDto(ubicacion);
    }

    public async Task<UbicacionDto> CreateUbicacionAsync(CreateUbicacionRequest request, int usuarioId)
    {
        // Validar código único
        if (await _context.InventarioUbicaciones.AnyAsync(u => u.Codigo == request.Codigo))
            throw new ArgumentException($"Ya existe una ubicación con el código {request.Codigo}");

        var ubicacion = new InventarioUbicacione
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            Tipo = request.Tipo,
            FkUbicacionPadre = request.UbicacionPadreId,
            CapacidadMaxima = request.CapacidadMaxima,
            Activo = request.Activo
        };

        _context.InventarioUbicaciones.Add(ubicacion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UsuarioId} creó ubicación {Codigo}", usuarioId, request.Codigo);
        return MapToUbicacionDto(ubicacion);
    }

    public async Task<UbicacionDto> UpdateUbicacionAsync(UpdateUbicacionRequest request, int usuarioId)
    {
        var ubicacion = await _context.InventarioUbicaciones.FindAsync(request.Id);
        if (ubicacion == null)
            throw new ArgumentException("Ubicación no encontrada");

        // Validar código único si cambió
        if (ubicacion.Codigo != request.Codigo &&
            await _context.InventarioUbicaciones.AnyAsync(u => u.Codigo == request.Codigo && u.Id != request.Id))
            throw new ArgumentException($"Ya existe otra ubicación con el código {request.Codigo}");

        ubicacion.Codigo = request.Codigo;
        ubicacion.Nombre = request.Nombre;
        ubicacion.Tipo = request.Tipo;
        ubicacion.FkUbicacionPadre = request.UbicacionPadreId;
        ubicacion.CapacidadMaxima = request.CapacidadMaxima;
        ubicacion.Activo = request.Activo;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Usuario {UsuarioId} actualizó ubicación {Id}", usuarioId, request.Id);

        return MapToUbicacionDto(ubicacion);
    }

    public async Task<bool> DeleteUbicacionAsync(int id)
    {
        var ubicacion = await _context.InventarioUbicaciones
            .Include(u => u.InverseFkUbicacionPadreNavigation)
            .Include(u => u.NumeroSerieProductos)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (ubicacion == null)
            return false;

        // Verificar que no tenga hijos ni productos asignados
        if (ubicacion.InverseFkUbicacionPadreNavigation.Any() || ubicacion.NumeroSerieProductos.Any())
            throw new InvalidOperationException("No se puede eliminar una ubicación que tiene sub-ubicaciones o productos asignados.");

        _context.InventarioUbicaciones.Remove(ubicacion);
        await _context.SaveChangesAsync();
        return true;
    }

    // ========== Movimientos ==========
    public async Task<List<MovimientoInventarioDto>> GetMovimientosAsync(int? productoId = null, DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.InventarioMovimientos
            .Include(m => m.FkProductoNavigation)
            .Include(m => m.FkUsuarioNavigation)
            .AsQueryable();

        if (productoId.HasValue)
            query = query.Where(m => m.FkProducto == productoId);

        if (desde.HasValue)
            query = query.Where(m => m.FechaMovimiento >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(m => m.FechaMovimiento <= hasta.Value);

        var movimientos = await query.OrderByDescending(m => m.FechaMovimiento).ToListAsync();

        return movimientos.Select(MapToMovimientoDto).ToList();
    }

    public async Task<MovimientoInventarioDto> RegistrarEntradaAsync(CreateMovimientoRequest request, int usuarioId)
    {
        return await RegistrarMovimiento(request, "Entrada", usuarioId);
    }

    public async Task<MovimientoInventarioDto> RegistrarSalidaAsync(CreateMovimientoRequest request, int usuarioId)
    {
        return await RegistrarMovimiento(request, "Salida", usuarioId);
    }

    public async Task<MovimientoInventarioDto> RegistrarAjusteAsync(CreateMovimientoRequest request, int usuarioId)
    {
        return await RegistrarMovimiento(request, "Ajuste", usuarioId);
    }

    public async Task<MovimientoInventarioDto> RegistrarDevolucionAsync(CreateMovimientoRequest request, int usuarioId)
    {
        return await RegistrarMovimiento(request, "Devolucion", usuarioId);
    }

    private async Task<MovimientoInventarioDto> RegistrarMovimiento(CreateMovimientoRequest request, string tipoMovimiento, int usuarioId)
    {
        // Validar producto
        var producto = await _context.Productos.FindAsync(request.ProductoId);
        if (producto == null)
            throw new ArgumentException("Producto no encontrado");

        // Calcular cantidad actual (suma de números de serie en estado 'Se_Puede_Vender')
        var cantidadActual = await _context.NumeroSerieProductos
            .CountAsync(ns => ns.FkProducto == request.ProductoId && ns.EstadoInventario == "Se_Puede_Vender");

        int nuevaCantidad;
        if (tipoMovimiento == "Entrada" || tipoMovimiento == "Devolucion")
            nuevaCantidad = cantidadActual + request.Cantidad;
        else if (tipoMovimiento == "Salida")
        {
            if (cantidadActual < request.Cantidad)
                throw new InvalidOperationException("Stock insuficiente");
            nuevaCantidad = cantidadActual - request.Cantidad;
        }
        else // Ajuste
            nuevaCantidad = request.Cantidad; // En ajuste, Cantidad es la nueva cantidad absoluta

        // Crear movimiento
        var movimiento = new InventarioMovimiento
        {
            FkProducto = request.ProductoId,
            FkUsuario = usuarioId,
            TipoMovimiento = tipoMovimiento,
            Cantidad = request.Cantidad,
            CantidadAnterior = cantidadActual,
            CantidadNueva = nuevaCantidad,
            Motivo = request.Motivo,
            Referencia = request.Referencia,
            CostoUnitario = request.CostoUnitario,
            FechaMovimiento = DateTime.Now
        };

        _context.InventarioMovimientos.Add(movimiento);

        // Si es entrada y se proporcionan números de serie, crearlos
        if ((tipoMovimiento == "Entrada" || tipoMovimiento == "Devolucion") && request.NumerosSerie != null)
        {
            foreach (var nsReq in request.NumerosSerie)
            {
                // Validar número de serie único
                if (await _context.NumeroSerieProductos.AnyAsync(ns => ns.NumeroSerie == nsReq.NumeroSerie))
                    throw new ArgumentException($"El número de serie {nsReq.NumeroSerie} ya existe");

                var numeroSerie = new NumeroSerieProducto
                {
                    FkProducto = request.ProductoId,
                    NumeroSerie = nsReq.NumeroSerie,
                    EstadoInventario = "Se_Puede_Vender",
                    FechaIngreso = DateTime.Now,
                    FkProveedor = nsReq.ProveedorId,
                    FkUbicacion = nsReq.UbicacionId
                };
                _context.NumeroSerieProductos.Add(numeroSerie);
            }
        }

        // Si es salida, marcar números de serie como vendidos (aquí simplificado, se podría elegir qué números específicos)
        if (tipoMovimiento == "Salida")
        {
            var numerosAVender = await _context.NumeroSerieProductos
                .Where(ns => ns.FkProducto == request.ProductoId && ns.EstadoInventario == "Se_Puede_Vender")
                .OrderBy(ns => ns.FechaIngreso) // FIFO
                .Take(request.Cantidad)
                .ToListAsync();

            foreach (var ns in numerosAVender)
            {
                ns.EstadoInventario = "Vendido";
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UsuarioId} registró movimiento {Tipo} de {Cantidad} unidades del producto {ProductoId}",
            usuarioId, tipoMovimiento, request.Cantidad, request.ProductoId);

        return MapToMovimientoDto(movimiento);
    }

    // ========== Números de serie ==========
    public async Task<List<NumeroSerieDto>> GetNumerosSerieAsync(int? productoId = null, string? estado = null, int? ubicacionId = null)
    {
        var query = _context.NumeroSerieProductos
            .Include(ns => ns.FkProductoNavigation)
            .Include(ns => ns.FkProveedorNavigation)
            .Include(ns => ns.FkUbicacionNavigation)
            .AsQueryable();

        if (productoId.HasValue)
            query = query.Where(ns => ns.FkProducto == productoId);

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(ns => ns.EstadoInventario == estado);

        if (ubicacionId.HasValue)
            query = query.Where(ns => ns.FkUbicacion == ubicacionId);

        var numeros = await query.OrderBy(ns => ns.NumeroSerie).ToListAsync();
        return numeros.Select(MapToNumeroSerieDto).ToList();
    }

    public async Task<NumeroSerieDto?> GetNumeroSerieByNumeroAsync(string numeroSerie)
    {
        var ns = await _context.NumeroSerieProductos
            .Include(n => n.FkProductoNavigation)
            .Include(n => n.FkProveedorNavigation)
            .Include(n => n.FkUbicacionNavigation)
            .FirstOrDefaultAsync(n => n.NumeroSerie == numeroSerie);

        return ns == null ? null : MapToNumeroSerieDto(ns);
    }

    public async Task<bool> UpdateNumeroSerieAsync(UpdateNumeroSerieRequest request, int usuarioId)
    {
        var ns = await _context.NumeroSerieProductos.FindAsync(request.Id);
        if (ns == null)
            return false;

        ns.FkUbicacion = request.UbicacionId;
        ns.EstadoInventario = request.EstadoInventario;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Usuario {UsuarioId} actualizó número de serie {Id}", usuarioId, request.Id);
        return true;
    }

    // ========== Proveedores ==========
    public async Task<List<ProveedorDto>> GetAllProveedoresAsync(bool soloActivos = true)
    {
        var query = _context.Proveedores.AsQueryable();
        if (soloActivos)
            query = query.Where(p => (bool)p.Activo);

        var proveedores = await query.OrderBy(p => p.Nombre).ToListAsync();
        return proveedores.Select(MapToProveedorDto).ToList();
    }

    public async Task<ProveedorDto?> GetProveedorByIdAsync(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        return proveedor == null ? null : MapToProveedorDto(proveedor);
    }

    public async Task<ProveedorDto> CreateProveedorAsync(CreateProveedorRequest request, int usuarioId)
    {
        // Validar RUC único
        if (await _context.Proveedores.AnyAsync(p => p.Ruc == request.Ruc))
            throw new ArgumentException($"Ya existe un proveedor con RUC {request.Ruc}");

        var proveedor = new Proveedore
        {
            Nombre = request.Nombre,
            Cedula = request.Cedula,
            Ruc = request.Ruc,
            Direccion = request.Direccion,
            Telefono = request.Telefono,
            Email = request.Email,
            ContactoPrincipal = request.ContactoPrincipal,
            PlazoEntregaDias = request.PlazoEntregaDias,
            Activo = request.Activo,
            FechaCreacion = DateTime.Now
        };

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UsuarioId} creó proveedor {Nombre}", usuarioId, request.Nombre);
        return MapToProveedorDto(proveedor);
    }

    public async Task<ProveedorDto> UpdateProveedorAsync(UpdateProveedorRequest request, int usuarioId)
    {
        var proveedor = await _context.Proveedores.FindAsync(request.Id);
        if (proveedor == null)
            throw new ArgumentException("Proveedor no encontrado");

        // Validar RUC único si cambió
        if (proveedor.Ruc != request.Ruc &&
            await _context.Proveedores.AnyAsync(p => p.Ruc == request.Ruc && p.Id != request.Id))
            throw new ArgumentException($"Ya existe otro proveedor con RUC {request.Ruc}");

        proveedor.Nombre = request.Nombre;
        proveedor.Cedula = request.Cedula;
        proveedor.Ruc = request.Ruc;
        proveedor.Direccion = request.Direccion;
        proveedor.Telefono = request.Telefono;
        proveedor.Email = request.Email;
        proveedor.ContactoPrincipal = request.ContactoPrincipal;
        proveedor.PlazoEntregaDias = request.PlazoEntregaDias;
        proveedor.Activo = request.Activo;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Usuario {UsuarioId} actualizó proveedor {Id}", usuarioId, request.Id);
        return MapToProveedorDto(proveedor);
    }

    public async Task<bool> ToggleProveedorActivoAsync(int id, bool activo)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
            return false;

        proveedor.Activo = activo;
        await _context.SaveChangesAsync();
        return true;
    }

    // ========== Mapeos privados ==========
    private UbicacionDto MapToUbicacionDto(InventarioUbicacione u)
    {
        return new UbicacionDto
        {
            Id = u.Id,
            Codigo = u.Codigo,
            Nombre = u.Nombre,
            Tipo = u.Tipo,
            UbicacionPadreId = u.FkUbicacionPadre,
            UbicacionPadreNombre = u.FkUbicacionPadreNavigation?.Nombre,
            CapacidadMaxima = u.CapacidadMaxima,
            Activo = u.Activo ?? true
        };
    }

    private MovimientoInventarioDto MapToMovimientoDto(InventarioMovimiento m)
    {
        return new MovimientoInventarioDto
        {
            Id = m.Id,
            ProductoId = m.FkProducto,
            ProductoNombre = m.FkProductoNavigation?.Modelo ?? "", // CORREGIDO: usar Modelo en lugar de NombreCompleto
            ProductoSku = m.FkProductoNavigation?.Sku ?? "",
            UsuarioId = m.FkUsuario,
            UsuarioNombre = m.FkUsuarioNavigation != null ? $"{m.FkUsuarioNavigation.Nombres} {m.FkUsuarioNavigation.Apellidos}" : "",
            TipoMovimiento = m.TipoMovimiento,
            Cantidad = m.Cantidad,
            CantidadAnterior = m.CantidadAnterior,
            CantidadNueva = m.CantidadNueva,
            Motivo = m.Motivo,
            Referencia = m.Referencia,
            FechaMovimiento = m.FechaMovimiento ?? DateTime.Now,
            CostoUnitario = m.CostoUnitario
        };
    }

    private NumeroSerieDto MapToNumeroSerieDto(NumeroSerieProducto ns)
    {
        return new NumeroSerieDto
        {
            Id = ns.Id,
            ProductoId = ns.FkProducto,
            ProductoNombre = ns.FkProductoNavigation?.Modelo ?? "", // CORREGIDO: usar Modelo en lugar de NombreCompleto
            NumeroSerie = ns.NumeroSerie,
            EstadoInventario = ns.EstadoInventario,
            FechaIngreso = ns.FechaIngreso ?? DateTime.Now,
            ProveedorId = ns.FkProveedor,
            ProveedorNombre = ns.FkProveedorNavigation?.Nombre ?? "",
            UbicacionId = ns.FkUbicacion,
            UbicacionNombre = ns.FkUbicacionNavigation?.Nombre
        };
    }

    private ProveedorDto MapToProveedorDto(Proveedore p)
    {
        return new ProveedorDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Cedula = p.Cedula,
            Ruc = p.Ruc,
            Direccion = p.Direccion,
            Telefono = p.Telefono,
            Email = p.Email,
            ContactoPrincipal = p.ContactoPrincipal,
            PlazoEntregaDias = p.PlazoEntregaDias,
            Activo = p.Activo ?? true,
            FechaCreacion = p.FechaCreacion ?? DateTime.Now
        };
    }
}