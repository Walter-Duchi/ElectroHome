using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using Application.DTOs.Tecnico;
using Infrastructure.Models;
using System.IO;
using Microsoft.Extensions.Configuration;
using Infrastructure.Interfaces;

namespace Infrastructure.Services
{
    public class TecnicoService : ITecnicoService
    {
        private readonly ReclamosContext _context;
        private readonly ILogger<TecnicoService> _logger;
        private readonly IConfiguration _configuration;

        public TecnicoService(ReclamosContext context, ILogger<TecnicoService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        private static bool CalcularGarantiaValida(DateTime? fechaVenta, int diasGarantia)
        {
            if (!fechaVenta.HasValue || diasGarantia <= 0)
            {
                return false;
            }

            var fechaLimite = fechaVenta.Value.AddDays(diasGarantia);
            var hoy = DateTime.Now;
            return hoy <= fechaLimite;
        }

        public async Task<List<TecnicoProductosResponse>> ObtenerProductosAsignadosAsync(int tecnicoId)
        {
            using var activity = new System.Diagnostics.Activity("ObtenerProductosAsignados");
            activity.Start();
            
            try
            {
                _logger.LogInformation("=== INICIO ObtenerProductosAsignadosAsync ===");
                _logger.LogInformation("Técnico ID: {TecnicoId}", tecnicoId);

                // Verificar si el técnico existe
                var tecnicoExiste = await _context.Usuarios.AnyAsync(u => u.Id == tecnicoId && u.Rol == "Tecnico");
                _logger.LogInformation("Técnico existe en BD: {Existe}", tecnicoExiste);

                if (!tecnicoExiste)
                {
                    _logger.LogWarning("Técnico ID {TecnicoId} no encontrado o no es técnico", tecnicoId);
                    return new List<TecnicoProductosResponse>();
                }

                // Query detallada con logging
                var query = _context.ReclamosProductoSns
                    .Include(rps => rps.FkNumeroSerieProductosNavigation)
                        .ThenInclude(nsp => nsp.FkProductoNavigation)
                            .ThenInclude(p => p.FkMarcaNavigation)
                    .Include(rps => rps.FkReclamosNavigation)
                        .ThenInclude(r => r.FkEmpresaClienteNavigation)
                    .Where(rps => rps.FkTecnicoAsignado == tecnicoId &&
                                 (rps.Estado == "Pendiente" || rps.Estado == "En Revision"));

                try
                {
                    _logger.LogInformation("Query SQL generada: {Query}", query.ToQueryString());
                }
                catch
                {
                    _logger.LogDebug("No se pudo obtener QueryString.");
                }

                _logger.LogInformation("Total de registros encontrados antes de selección: {Count}", 
                    await query.CountAsync());

                // Primero obtener datos básicos sin el cálculo complejo
                var queryResult = await query
                    .OrderBy(rps => rps.FechaReclamoClienteFinal)
                    .Select(rps => new 
                    {
                        Id = rps.Id,
                        NumeroSerie = rps.FkNumeroSerieProductosNavigation.NumeroSerie,
                        Marca = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.FkMarcaNavigation.Nombre,
                        Modelo = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.Modelo,
                        Especificacion = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.Especificacion,
                        Estado = rps.Estado,
                        FechaReclamoClienteFinal = rps.FechaReclamoClienteFinal ?? DateTime.MinValue,
                        CodigoReclamo = rps.FkReclamosNavigation.CodigoReclamo,
                        FormaCompensacion = rps.FormaCompensacion,
                        Precio = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.Precio,
                        ClienteNombre = $"{rps.FkReclamosNavigation.FkEmpresaClienteNavigation.Nombres} {rps.FkReclamosNavigation.FkEmpresaClienteNavigation.Apellidos}",
                        ClienteRuc = rps.FkReclamosNavigation.FkEmpresaClienteNavigation.Ruc,
                        FechaVentaClienteFinal = rps.FechaVentaClienteFinal,
                        DiasGarantia = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.DiasGarantia
                    })
                    .ToListAsync();

                // Ahora calcular GarantiaValida en memoria
                var productos = queryResult.Select(r => new TecnicoProductosResponse
                {
                    Id = r.Id,
                    NumeroSerie = r.NumeroSerie,
                    Marca = r.Marca,
                    Modelo = r.Modelo,
                    Especificacion = r.Especificacion,
                    Estado = r.Estado,
                    FechaReclamoClienteFinal = r.FechaReclamoClienteFinal,
                    CodigoReclamo = r.CodigoReclamo,
                    FormaCompensacion = r.FormaCompensacion,
                    Precio = r.Precio,
                    ClienteNombre = r.ClienteNombre,
                    ClienteRuc = r.ClienteRuc,
                    FechaVentaClienteFinal = r.FechaVentaClienteFinal,
                    DiasGarantia = r.DiasGarantia,
                    GarantiaValida = CalcularGarantiaValida(r.FechaVentaClienteFinal, r.DiasGarantia)
                }).ToList();

                _logger.LogInformation("Productos obtenidos: {Cantidad}", productos.Count);
                
                if (productos.Count > 0)
                {
                    _logger.LogInformation("Detalle de productos:");
                    foreach (var p in productos)
                    {
                        _logger.LogInformation("  - ID: {Id}, Serie: {Serie}, Estado: {Estado}, Marca: {Marca}", 
                            p.Id, p.NumeroSerie, p.Estado, p.Marca);
                    }
                }

                _logger.LogInformation("=== FIN ObtenerProductosAsignadosAsync ===");
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR en ObtenerProductosAsignadosAsync para técnico ID: {TecnicoId}", tecnicoId);
                _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
                _logger.LogError("Inner Exception: {InnerException}", ex.InnerException?.Message);
                
                throw new Exception($"Error al obtener productos asignados: {ex.Message}", ex);
            }
        }

        public async Task<TecnicoProductosResponse?> ObtenerProximoProductoAsync(int tecnicoId)
        {
            try
            {
                _logger.LogInformation("Obteniendo próximo producto para técnico ID: {TecnicoId}", tecnicoId);

                var queryResult = await _context.ReclamosProductoSns
                    .Include(rps => rps.FkNumeroSerieProductosNavigation)
                        .ThenInclude(nsp => nsp.FkProductoNavigation)
                            .ThenInclude(p => p.FkMarcaNavigation)
                    .Include(rps => rps.FkReclamosNavigation)
                        .ThenInclude(r => r.FkEmpresaClienteNavigation)
                    .Where(rps => rps.FkTecnicoAsignado == tecnicoId &&
                                 rps.Estado == "Pendiente")
                    .OrderBy(rps => rps.FechaReclamoClienteFinal)
                    .Select(rps => new 
                    {
                        Id = rps.Id,
                        NumeroSerie = rps.FkNumeroSerieProductosNavigation.NumeroSerie,
                        Marca = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.FkMarcaNavigation.Nombre,
                        Modelo = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.Modelo,
                        Especificacion = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.Especificacion,
                        Estado = rps.Estado,
                        FechaReclamoClienteFinal = rps.FechaReclamoClienteFinal ?? DateTime.MinValue,
                        CodigoReclamo = rps.FkReclamosNavigation.CodigoReclamo,
                        FormaCompensacion = rps.FormaCompensacion,
                        Precio = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.Precio,
                        ClienteNombre = $"{rps.FkReclamosNavigation.FkEmpresaClienteNavigation.Nombres} {rps.FkReclamosNavigation.FkEmpresaClienteNavigation.Apellidos}",
                        ClienteRuc = rps.FkReclamosNavigation.FkEmpresaClienteNavigation.Ruc,
                        FechaVentaClienteFinal = rps.FechaVentaClienteFinal,
                        DiasGarantia = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.DiasGarantia
                    })
                    .FirstOrDefaultAsync();

                if (queryResult == null)
                {
                    _logger.LogInformation("No hay productos pendientes para técnico ID: {TecnicoId}", tecnicoId);
                    return null;
                }

                _logger.LogInformation("Próximo producto encontrado: {ProductoId} - {NumeroSerie}",
                    queryResult.Id, queryResult.NumeroSerie);

                return new TecnicoProductosResponse
                {
                    Id = queryResult.Id,
                    NumeroSerie = queryResult.NumeroSerie,
                    Marca = queryResult.Marca,
                    Modelo = queryResult.Modelo,
                    Especificacion = queryResult.Especificacion,
                    Estado = queryResult.Estado,
                    FechaReclamoClienteFinal = queryResult.FechaReclamoClienteFinal,
                    CodigoReclamo = queryResult.CodigoReclamo,
                    FormaCompensacion = queryResult.FormaCompensacion,
                    Precio = queryResult.Precio,
                    ClienteNombre = queryResult.ClienteNombre,
                    ClienteRuc = queryResult.ClienteRuc,
                    FechaVentaClienteFinal = queryResult.FechaVentaClienteFinal,
                    DiasGarantia = queryResult.DiasGarantia,
                    GarantiaValida = CalcularGarantiaValida(queryResult.FechaVentaClienteFinal, queryResult.DiasGarantia)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener próximo producto para técnico ID: {TecnicoId}", tecnicoId);
                throw;
            }
        }

        public async Task<bool> IniciarRevisionAsync(IniciarRevisionRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Iniciando revisión para producto ID: {ProductoId} por técnico ID: {TecnicoId}",
                    request.ReclamoProductoSnId, request.TecnicoId);

                // 1. Validar que el técnico esté asignado a este producto
                var producto = await _context.ReclamosProductoSns
                    .FirstOrDefaultAsync(rps => rps.Id == request.ReclamoProductoSnId &&
                                               rps.FkTecnicoAsignado == request.TecnicoId);

                if (producto == null)
                {
                    _logger.LogError("Producto {ProductoId} no encontrado o no asignado al técnico {TecnicoId}",
                        request.ReclamoProductoSnId, request.TecnicoId);
                    return false;
                }

                // 2. Validar que el producto esté en estado Pendiente
                if (producto.Estado != "Pendiente")
                {
                    _logger.LogError("Producto {ProductoId} no está en estado Pendiente. Estado actual: {Estado}",
                        request.ReclamoProductoSnId, producto.Estado);
                    return false;
                }

                // 3. Validar que no haya otro producto en revisión por este técnico
                var tieneRevisionActiva = await _context.ReclamosProductoSns
                    .AnyAsync(rps => rps.FkTecnicoAsignado == request.TecnicoId &&
                                    rps.Estado == "En Revision");

                if (tieneRevisionActiva)
                {
                    _logger.LogError("El técnico {TecnicoId} ya tiene una revisión activa", request.TecnicoId);
                    return false;
                }

                // 4. Validar orden de revisión (debe ser el producto más antiguo pendiente)
                var productoMasAntiguo = await _context.ReclamosProductoSns
                    .Where(rps => rps.FkTecnicoAsignado == request.TecnicoId &&
                                 rps.Estado == "Pendiente")
                    .OrderBy(rps => rps.FechaReclamoClienteFinal)
                    .FirstOrDefaultAsync();

                if (productoMasAntiguo == null || productoMasAntiguo.Id != request.ReclamoProductoSnId)
                {
                    _logger.LogError("El producto {ProductoId} no es el más antiguo pendiente. No se puede revisar fuera de orden.",
                        request.ReclamoProductoSnId);
                    return false;
                }

                // 5. Actualizar estado a "En Revision"
                producto.Estado = "En Revision";
                producto.FechaRevisionTecnico = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Revisión iniciada exitosamente para producto ID: {ProductoId}", request.ReclamoProductoSnId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al iniciar revisión para producto ID: {ProductoId}", request.ReclamoProductoSnId);
                throw;
            }
        }

        public async Task<bool> FinalizarRevisionAsync(FinalizarRevisionRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Finalizando revisión para producto ID: {ProductoId} por técnico ID: {TecnicoId}",
                    request.ReclamoProductoSnId, request.TecnicoId);

                // 1. Validar que el técnico esté asignado a este producto
                var producto = await _context.ReclamosProductoSns
                    .FirstOrDefaultAsync(rps => rps.Id == request.ReclamoProductoSnId &&
                                               rps.FkTecnicoAsignado == request.TecnicoId);

                if (producto == null)
                {
                    _logger.LogError("Producto {ProductoId} no encontrado o no asignado al técnico {TecnicoId}",
                        request.ReclamoProductoSnId, request.TecnicoId);
                    return false;
                }

                // 2. Validar que el producto esté en estado "En Revision"
                if (producto.Estado != "En Revision")
                {
                    _logger.LogError("Producto {ProductoId} no está en estado 'En Revision'. Estado actual: {Estado}",
                        request.ReclamoProductoSnId, producto.Estado);
                    return false;
                }

                // 3. Validar que el estado sea "Aprobado" o "Rechazado"
                if (request.Estado != "Aprobado" && request.Estado != "Rechazado")
                {
                    _logger.LogError("Estado inválido: {Estado}. Debe ser 'Aprobado' o 'Rechazado'", request.Estado);
                    return false;
                }

                // 4. Validar que la explicación no esté vacía
                if (string.IsNullOrWhiteSpace(request.Explicacion))
                {
                    _logger.LogError("La explicación es requerida");
                    return false;
                }

                // 5. Guardar PDF si se proporciona
                string? rutaPdf = null;
                if (!string.IsNullOrWhiteSpace(request.PdfBase64) && !string.IsNullOrWhiteSpace(request.PdfFileName))
                {
                    rutaPdf = await GuardarPdfAsync(request.PdfBase64, request.PdfFileName, request.ReclamoProductoSnId);
                }

                // 6. Actualizar el producto
                producto.Estado = request.Estado;
                producto.ExplicacionRespuestaTecnico = request.Explicacion;
                producto.PdfRevisionTecnico = rutaPdf;
                producto.FechaRevisionTecnico = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Revisión finalizada exitosamente para producto ID: {ProductoId}. Estado: {Estado}",
                    request.ReclamoProductoSnId, request.Estado);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al finalizar revisión para producto ID: {ProductoId}", request.ReclamoProductoSnId);
                throw;
            }
        }

        public async Task<bool> ValidarOrdenRevisacionAsync(int reclamoProductoSnId, int tecnicoId)
        {
            try
            {
                var producto = await _context.ReclamosProductoSns
                    .FirstOrDefaultAsync(rps => rps.Id == reclamoProductoSnId &&
                                               rps.FkTecnicoAsignado == tecnicoId);

                if (producto == null || producto.Estado != "Pendiente")
                {
                    return false;
                }

                // Verificar si es el producto más antiguo pendiente
                var productoMasAntiguo = await _context.ReclamosProductoSns
                    .Where(rps => rps.FkTecnicoAsignado == tecnicoId &&
                                 rps.Estado == "Pendiente")
                    .OrderBy(rps => rps.FechaReclamoClienteFinal)
                    .Select(rps => rps.Id)
                    .FirstOrDefaultAsync();

                return productoMasAntiguo == reclamoProductoSnId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar orden de revisación para producto ID: {ProductoId}", reclamoProductoSnId);
                return false;
            }
        }

        private async Task<string> GuardarPdfAsync(string pdfBase64, string fileName, int reclamoProductoSnId)
        {
            try
            {
                var pdfSettings = _configuration.GetSection("PdfSettings");
                var basePath = pdfSettings["StoragePath"] ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "reclamos",
                    "tecnico");

                // Crear directorio si no existe
                Directory.CreateDirectory(basePath);

                // Generar nombre único para el archivo
                var uniqueFileName = $"{reclamoProductoSnId}_{DateTime.Now:yyyyMMddHHmmss}_{fileName}";
                var filePath = Path.Combine(basePath, uniqueFileName);

                // Decodificar base64 y guardar archivo
                var bytes = Convert.FromBase64String(pdfBase64);
                await File.WriteAllBytesAsync(filePath, bytes);

                _logger.LogInformation("PDF guardado en: {FilePath}", filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar PDF para producto ID: {ProductoId}", reclamoProductoSnId);
                throw;
            }
        }
    }
}