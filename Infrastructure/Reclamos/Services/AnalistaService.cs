using Application.DTOs.Analista;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace Infrastructure.Reclamos.Services;

public class AnalistaService : IAnalistaService
{
    private readonly ReclamosContext _context;
    private readonly ILogger<AnalistaService> _logger;

    public AnalistaService(ReclamosContext context, ILogger<AnalistaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashboardAnalistaDto> ObtenerDashboardAsync()
    {
        var hoy = DateTime.Today;
        var hace30Dias = hoy.AddDays(-30);
        var hace30DiasAnterior = hace30Dias.AddDays(-30);
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        var ventas = await _context.Ventas
            .Where(v => v.FechaCompra >= hace30Dias && v.FechaCompra <= hoy && v.EstadoSri == "Autorizado")
            .Include(v => v.VentasPorNumeroSerieProductos)
                .ThenInclude(vp => vp.FkNumeroSerieProductoNavigation)
                    .ThenInclude(ns => ns.FkProductoNavigation)
                        .ThenInclude(p => p.FkMarcaNavigation)
            .Include(v => v.VentasPorNumeroSerieProductos)
                .ThenInclude(vp => vp.FkNumeroSerieProductoNavigation)
                    .ThenInclude(ns => ns.FkProductoNavigation)
                        .ThenInclude(p => p.FkCategoriaNavigation)
            .ToListAsync();

        var ventasAnteriores = await _context.Ventas
            .Where(v => v.FechaCompra >= hace30DiasAnterior && v.FechaCompra < hace30Dias && v.EstadoSri == "Autorizado")
            .ToListAsync();

        var ventasDiarias = ventas
            .Where(v => v.FechaCompra.HasValue)
            .GroupBy(v => v.FechaCompra!.Value.Date)
            .Select(g => new VentaDiariaDto
            {
                Fecha = g.Key,
                Total = g.Sum(v => v.TotalCompra),
                CantidadVentas = g.Count()
            })
            .OrderBy(v => v.Fecha)
            .ToList();

        var productosVendidos = ventas
            .SelectMany(v => v.VentasPorNumeroSerieProductos ?? new List<VentasPorNumeroSerieProducto>())
            .Where(vp => vp.FkNumeroSerieProductoNavigation?.FkProductoNavigation != null)
            .GroupBy(vp => vp.FkNumeroSerieProductoNavigation.FkProducto)
            .Select(g => new ProductoMasVendidoDto
            {
                ProductoId = g.Key,
                NombreProducto = g.First().FkNumeroSerieProductoNavigation.FkProductoNavigation.Modelo ?? "Sin nombre",
                Marca = g.First().FkNumeroSerieProductoNavigation.FkProductoNavigation.FkMarcaNavigation?.Nombre ?? "Sin marca",
                UnidadesVendidas = g.Count(),
                IngresoGenerado = g.Sum(vp => vp.PrecioVenta)
            })
            .OrderByDescending(p => p.UnidadesVendidas)
            .Take(10)
            .ToList();

        var ventasPorCategoria = ventas
            .SelectMany(v => v.VentasPorNumeroSerieProductos ?? new List<VentasPorNumeroSerieProducto>())
            .Where(vp => vp.FkNumeroSerieProductoNavigation?.FkProductoNavigation != null)
            .GroupBy(vp => vp.FkNumeroSerieProductoNavigation.FkProductoNavigation.FkCategoria ?? 0)
            .Select(g => new CategoriaVentasDto
            {
                CategoriaId = g.Key,
                NombreCategoria = g.First().FkNumeroSerieProductoNavigation.FkProductoNavigation.FkCategoriaNavigation?.Nombre ?? "Sin categoría",
                UnidadesVendidas = g.Count(),
                IngresoGenerado = g.Sum(vp => vp.PrecioVenta)
            })
            .ToList();

        var totalIngresos = ventas.Sum(v => v.TotalCompra);
        foreach (var cat in ventasPorCategoria)
        {
            cat.PorcentajeVentas = totalIngresos > 0 ? (cat.IngresoGenerado / totalIngresos) * 100 : 0;
        }

        var reclamos = await _context.ReclamosProductoSns
            .Where(r => r.FechaReclamoClienteFinal >= hace30Dias)
            .ToListAsync();

        var reclamosPorEstado = reclamos
            .GroupBy(r => r.Estado ?? "Desconocido")
            .Select(g => new ReclamoEstadoDto
            {
                Estado = g.Key,
                Cantidad = g.Count(),
                Porcentaje = reclamos.Count > 0 ? (g.Count() * 100.0m / reclamos.Count) : 0
            })
            .ToList();

        var productosConStock = await _context.NumeroSerieProductos
            .Where(ns => ns.EstadoInventario == "Se_Puede_Vender")
            .GroupBy(ns => ns.FkProducto)
            .Select(g => new { ProductoId = g.Key, Stock = g.Count() })
            .ToListAsync();

        var totalProductos = productosConStock.Count;
        var stockDisponible = productosConStock.Sum(p => p.Stock);

        var productosIds = productosConStock.Select(p => p.ProductoId).ToList();
        var productosInfo = await _context.Productos
            .Where(p => productosIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Modelo ?? "Producto");

        var productosBajoStock = productosConStock
            .Where(p => p.Stock < 5)
            .Select(p => new ProductoBajoStockDto
            {
                ProductoId = p.ProductoId,
                NombreProducto = productosInfo.GetValueOrDefault(p.ProductoId, "Desconocido"),
                StockActual = p.Stock,
                UmbralMinimo = 5
            })
            .ToList();

        var usuarios = await _context.Usuarios
            .Where(u => u.Activo == true)
            .ToListAsync();

        var usuariosPorRol = usuarios
            .GroupBy(u => u.Rol ?? "Sin rol")
            .Select(g => new UsuariosPorRolDto
            {
                Rol = g.Key,
                Cantidad = g.Count()
            })
            .ToList();

        var nuevosUltimoMes = await _context.Usuarios
            .CountAsync(u => u.FechaCreacion >= inicioMes);

        var totalProductosVendidos = productosVendidos.Sum(p => p.UnidadesVendidas);
        var reclamosPendientes = reclamos.Count(r => r.Estado == "Pendiente" || r.Estado == "En Revision");

        var resumen = new ResumenGeneralDto
        {
            TotalIngresos = totalIngresos,
            TotalVentas = ventas.Count,
            PromedioVenta = ventas.Any() ? totalIngresos / ventas.Count : 0,
            TotalProductosVendidos = totalProductosVendidos,
            ProductosEnInventario = stockDisponible,
            ReclamosPendientes = reclamosPendientes,
            UsuariosActivos = usuarios.Count
        };

        var ingresosPeriodoActual = totalIngresos;
        var ingresosPeriodoAnterior = ventasAnteriores.Sum(v => v.TotalCompra);
        var variacion = ingresosPeriodoAnterior > 0
            ? ((ingresosPeriodoActual - ingresosPeriodoAnterior) / ingresosPeriodoAnterior) * 100
            : 0;

        return new DashboardAnalistaDto
        {
            Resumen = resumen,
            VentasUltimos30Dias = new VentasPorPeriodoDto
            {
                VentasDiarias = ventasDiarias,
                TotalPeriodo = ingresosPeriodoActual,
                CantidadVentasPeriodo = ventas.Count,
                VariacionPorcentual = variacion
            },
            ProductosMasVendidos = productosVendidos,
            VentasPorCategoria = ventasPorCategoria,
            ReclamosPorEstado = reclamosPorEstado,
            Inventario = new InventarioResumenDto
            {
                TotalProductos = totalProductos,
                StockDisponible = stockDisponible,
                StockPorUbicacion = await _context.NumeroSerieProductos.CountAsync(ns => ns.FkUbicacion != null && ns.EstadoInventario == "Se_Puede_Vender"),
                ProductosBajoStock = productosBajoStock
            },
            Usuarios = new UsuariosActivosDto
            {
                Total = usuarios.Count,
                PorRol = usuariosPorRol,
                NuevosUltimoMes = nuevosUltimoMes
            }
        };
    }

    public async Task<byte[]> ExportarReporteVentasAsync(DateTime? desde, DateTime? hasta)
    {
        var fechaDesde = desde ?? DateTime.Today.AddDays(-30);
        var fechaHasta = hasta ?? DateTime.Today;

        var ventas = await _context.Ventas
            .Where(v => v.FechaCompra >= fechaDesde && v.FechaCompra <= fechaHasta && v.EstadoSri == "Autorizado")
            .Include(v => v.VentasPorNumeroSerieProductos)
            .ThenInclude(vp => vp.FkNumeroSerieProductoNavigation)
            .ThenInclude(ns => ns.FkProductoNavigation)
            .ThenInclude(p => p.FkMarcaNavigation)
            .OrderBy(v => v.FechaCompra)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Fecha;Número Factura;Cliente;Total;Productos;Monto Productos");
        foreach (var venta in ventas)
        {
            foreach (var detalle in venta.VentasPorNumeroSerieProductos)
            {
                sb.AppendLine($"{venta.FechaCompra:yyyy-MM-dd};{venta.CodigoFactura};{venta.FkEmpresaClienteNavigation?.Nombres ?? "N/A"};{venta.TotalCompra};{detalle.FkNumeroSerieProductoNavigation.FkProductoNavigation.Modelo};{detalle.PrecioVenta}");
            }
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportarReporteInventarioAsync()
    {
        var inventario = await _context.NumeroSerieProductos
            .Include(ns => ns.FkProductoNavigation)
            .ThenInclude(p => p.FkMarcaNavigation)
            .Include(ns => ns.FkUbicacionNavigation)
            .Where(ns => ns.EstadoInventario == "Se_Puede_Vender")
            .OrderBy(ns => ns.FkProductoNavigation.Modelo)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Producto;Marca;Número de Serie;Ubicación;Fecha Ingreso;Estado");
        foreach (var ns in inventario)
        {
            sb.AppendLine($"{ns.FkProductoNavigation.Modelo};{ns.FkProductoNavigation.FkMarcaNavigation.Nombre};{ns.NumeroSerie};{ns.FkUbicacionNavigation?.Nombre ?? "Sin ubicación"};{ns.FechaIngreso:yyyy-MM-dd};{ns.EstadoInventario}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}