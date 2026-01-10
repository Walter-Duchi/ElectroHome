using Application.DTOs.Entrega;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EntregaService : IEntregaService
    {
        private readonly ReclamosContext _context;
        private readonly ILogger<EntregaService> _logger;

        public EntregaService(ReclamosContext context, ILogger<EntregaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BuscarReclamoResponse> BuscarReclamoAsync(string codigoReclamo)
        {
            try
            {
                _logger.LogInformation($"Buscando reclamo: {codigoReclamo}");

                var reclamo = await _context.Reclamos
                    .Include(r => r.FkEmpresaClienteNavigation)
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.FkNumeroSerieProductosNavigation)
                            .ThenInclude(nsp => nsp.FkProductoNavigation)
                                .ThenInclude(p => p.FkMarcaNavigation)
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.ComprobanteProductoReemplazado)
                    .FirstOrDefaultAsync(r => r.CodigoReclamo == codigoReclamo);

                if (reclamo == null)
                {
                    return new BuscarReclamoResponse
                    {
                        Exito = false,
                        Mensaje = $"No se encontró el reclamo con código: {codigoReclamo}"
                    };
                }

                // Obtener todos los productos del reclamo
                var todosProductos = reclamo.ReclamosProductoSns.ToList();

                // Filtrar solo productos aprobados para reemplazo
                var productosParaEntrega = todosProductos
                    .Where(rps => rps.Estado == "Aprobado" && rps.FormaCompensacion == "Reemplazo")
                    .Select(rps => new ProductoEntregaDTO
                    {
                        ReclamoProductoSnId = rps.Id,
                        NumeroSerieProductoDefectuoso = rps.FkNumeroSerieProductosNavigation.NumeroSerie,
                        Marca = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.FkMarcaNavigation.Nombre,
                        Modelo = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.Modelo,
                        Estado = rps.Estado,
                        FormaCompensacion = rps.FormaCompensacion,
                        NumeroSerieReemplazo = rps.ComprobanteProductoReemplazado?.FkProductoDeReemplazoNavigation?.NumeroSerie,
                        ProductoReemplazoId = rps.ComprobanteProductoReemplazado?.FkProductoDeReemplazo,
                        ReemplazoValido = rps.ComprobanteProductoReemplazado != null,
                        MensajeValidacion = rps.ComprobanteProductoReemplazado != null
                            ? "Reemplazo asignado"
                            : "Requiere selección de producto de reemplazo"
                    }).ToList();

                // Contar productos pendientes de revisión
                var productosPendientesRevision = todosProductos
                    .Count(rps => rps.Estado != "Aprobado" && rps.Estado != "Rechazado" && rps.Estado != "Compensado");

                return new BuscarReclamoResponse
                {
                    Exito = true,
                    Mensaje = "Reclamo encontrado",
                    CodigoReclamo = reclamo.CodigoReclamo,
                    Cliente = $"{reclamo.FkEmpresaClienteNavigation.Nombres} {reclamo.FkEmpresaClienteNavigation.Apellidos}",
                    Ruc = reclamo.FkEmpresaClienteNavigation.Ruc,
                    Productos = productosParaEntrega,
                    TodosProductosRevisados = productosPendientesRevision == 0,
                    TotalProductosReclamo = todosProductos.Count,
                    ProductosPendientesRevision = productosPendientesRevision
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar reclamo: {codigoReclamo}");
                throw;
            }
        }

        public async Task<ValidarReemplazoResponse> ValidarProductoReemplazoAsync(int reclamoProductoSnId, string numeroSerieReemplazo)
        {
            try
            {
                _logger.LogInformation($"Validando producto de reemplazo: {numeroSerieReemplazo} para producto: {reclamoProductoSnId}");

                // Obtener el producto reclamado
                var productoReclamado = await _context.ReclamosProductoSns
                    .Include(rps => rps.FkNumeroSerieProductosNavigation)
                        .ThenInclude(nsp => nsp.FkProductoNavigation)
                            .ThenInclude(p => p.FkMarcaNavigation)
                    .FirstOrDefaultAsync(rps => rps.Id == reclamoProductoSnId);

                if (productoReclamado == null)
                {
                    return new ValidarReemplazoResponse
                    {
                        Valido = false,
                        Mensaje = "Producto reclamado no encontrado"
                    };
                }

                // Verificar que el producto reclamado está aprobado para reemplazo
                if (productoReclamado.Estado != "Aprobado" || productoReclamado.FormaCompensacion != "Reemplazo")
                {
                    return new ValidarReemplazoResponse
                    {
                        Valido = false,
                        Mensaje = "El producto no está aprobado para reemplazo"
                    };
                }

                // Buscar el producto de reemplazo
                var productoReemplazo = await _context.NumeroSerieProductos
                    .Include(nsp => nsp.FkProductoNavigation)
                        .ThenInclude(p => p.FkMarcaNavigation)
                    .FirstOrDefaultAsync(nsp => nsp.NumeroSerie == numeroSerieReemplazo);

                if (productoReemplazo == null)
                {
                    return new ValidarReemplazoResponse
                    {
                        Valido = false,
                        Mensaje = "Producto de reemplazo no encontrado"
                    };
                }

                // Verificar que el producto de reemplazo esté en estado "Se_Puede_Vender"
                if (productoReemplazo.EstadoInventario != "Se_Puede_Vender")
                {
                    return new ValidarReemplazoResponse
                    {
                        Valido = false,
                        Mensaje = $"El producto no está disponible para reemplazo. Estado actual: {productoReemplazo.EstadoInventario}"
                    };
                }

                // Verificar que sean de la misma marca y modelo
                var productoReclamadoMarcaId = productoReclamado.FkNumeroSerieProductosNavigation.FkProductoNavigation.FkMarca;
                var productoReclamadoModelo = productoReclamado.FkNumeroSerieProductosNavigation.FkProductoNavigation.Modelo;

                var productoReemplazoMarcaId = productoReemplazo.FkProductoNavigation.FkMarca;
                var productoReemplazoModelo = productoReemplazo.FkProductoNavigation.Modelo;

                if (productoReclamadoMarcaId != productoReemplazoMarcaId || productoReclamadoModelo != productoReemplazoModelo)
                {
                    return new ValidarReemplazoResponse
                    {
                        Valido = false,
                        Mensaje = "El producto de reemplazo no coincide en marca y modelo con el producto defectuoso"
                    };
                }

                // Verificar que el producto de reemplazo no esté ya asignado a otro reclamo
                var yaAsignado = await _context.ComprobanteProductoReemplazados
                    .AnyAsync(cpr => cpr.FkProductoDeReemplazo == productoReemplazo.Id);

                if (yaAsignado)
                {
                    return new ValidarReemplazoResponse
                    {
                        Valido = false,
                        Mensaje = "El producto de reemplazo ya ha sido asignado a otro reclamo"
                    };
                }

                return new ValidarReemplazoResponse
                {
                    Valido = true,
                    Mensaje = "Producto de reemplazo válido",
                    ProductoReemplazo = new ProductoReemplazoDTO
                    {
                        Id = productoReemplazo.Id,
                        NumeroSerie = productoReemplazo.NumeroSerie,
                        Marca = productoReemplazo.FkProductoNavigation.FkMarcaNavigation.Nombre,
                        Modelo = productoReemplazo.FkProductoNavigation.Modelo,
                        EstadoInventario = productoReemplazo.EstadoInventario
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al validar producto de reemplazo: {numeroSerieReemplazo}");
                throw;
            }
        }

        public async Task<bool> SeleccionarReemplazoAsync(SeleccionarReemplazoRequest request, int personalEntregaId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Seleccionando reemplazo para producto: {request.ReclamoProductoSnId}");

                // Validar el producto de reemplazo
                var validacion = await ValidarProductoReemplazoAsync(request.ReclamoProductoSnId, request.NumeroSerieReemplazo);

                if (!validacion.Valido)
                {
                    throw new InvalidOperationException(validacion.Mensaje);
                }

                var productoReclamado = await _context.ReclamosProductoSns
                    .Include(rps => rps.ComprobanteProductoReemplazado)
                    .FirstOrDefaultAsync(rps => rps.Id == request.ReclamoProductoSnId);

                if (productoReclamado == null)
                {
                    throw new InvalidOperationException("Producto reclamado no encontrado");
                }

                // Si ya tiene un reemplazo asignado, eliminar el anterior
                if (productoReclamado.ComprobanteProductoReemplazado != null)
                {
                    _context.ComprobanteProductoReemplazados.Remove(productoReclamado.ComprobanteProductoReemplazado);
                }

                // Crear la relación en ComprobanteProductoReemplazado
                var comprobanteProducto = new ComprobanteProductoReemplazado
                {
                    FkReclamosProductoSn = request.ReclamoProductoSnId,
                    FkProductoDeReemplazo = validacion.ProductoReemplazo!.Id
                    // FK_Comprobante_De_Reemplazo se asignará cuando se suba el PDF
                };

                _context.ComprobanteProductoReemplazados.Add(comprobanteProducto);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Reemplazo seleccionado exitosamente para producto: {request.ReclamoProductoSnId}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al seleccionar reemplazo para producto: {request.ReclamoProductoSnId}");
                throw;
            }
        }

        public async Task<ComprobanteEntregaDTO> GenerarDatosComprobanteAsync(string codigoReclamo, int personalEntregaId)
        {
            try
            {
                _logger.LogInformation($"Generando datos para comprobante de reclamo: {codigoReclamo}");

                var reclamo = await _context.Reclamos
                    .Include(r => r.FkEmpresaClienteNavigation)
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.FkNumeroSerieProductosNavigation)
                            .ThenInclude(nsp => nsp.FkProductoNavigation)
                                .ThenInclude(p => p.FkMarcaNavigation)
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.ComprobanteProductoReemplazado)
                            .ThenInclude(cpr => cpr.FkProductoDeReemplazoNavigation)
                    .FirstOrDefaultAsync(r => r.CodigoReclamo == codigoReclamo);

                if (reclamo == null)
                {
                    throw new InvalidOperationException($"Reclamo no encontrado: {codigoReclamo}");
                }

                var personalEntrega = await _context.Usuarios.FindAsync(personalEntregaId);
                if (personalEntrega == null)
                {
                    throw new InvalidOperationException("Personal de entrega no encontrado");
                }

                // Verificar que todos los productos tengan reemplazo asignado
                var productosSinReemplazo = reclamo.ReclamosProductoSns
                    .Where(rps => rps.Estado == "Aprobado" && rps.FormaCompensacion == "Reemplazo")
                    .Where(rps => rps.ComprobanteProductoReemplazado == null)
                    .ToList();

                if (productosSinReemplazo.Any())
                {
                    throw new InvalidOperationException("Todos los productos deben tener un reemplazo asignado antes de generar el comprobante");
                }

                var productosComprobante = reclamo.ReclamosProductoSns
                    .Where(rps => rps.Estado == "Aprobado" && rps.FormaCompensacion == "Reemplazo")
                    .Select(rps => new ProductoEntregaComprobanteDTO
                    {
                        NumeroSerieDefectuoso = rps.FkNumeroSerieProductosNavigation.NumeroSerie,
                        Marca = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.FkMarcaNavigation.Nombre,
                        Modelo = rps.FkNumeroSerieProductosNavigation.FkProductoNavigation.Modelo,
                        NumeroSerieReemplazo = rps.ComprobanteProductoReemplazado!.FkProductoDeReemplazoNavigation.NumeroSerie
                    }).ToList();

                return new ComprobanteEntregaDTO
                {
                    CodigoReclamo = reclamo.CodigoReclamo,
                    Cliente = $"{reclamo.FkEmpresaClienteNavigation.Nombres} {reclamo.FkEmpresaClienteNavigation.Apellidos}",
                    Ruc = reclamo.FkEmpresaClienteNavigation.Ruc,
                    FechaEntrega = DateTime.UtcNow,
                    PersonalEntrega = $"{personalEntrega.Nombres} {personalEntrega.Apellidos}",
                    Productos = productosComprobante,
                    FirmaBase64 = string.Empty // Se llenará en el frontend
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar datos para comprobante: {codigoReclamo}");
                throw;
            }
        }

        public async Task<string> GenerarPdfComprobanteAsync(ComprobanteEntregaDTO comprobante)
        {
            try
            {
                _logger.LogInformation($"Generando PDF para comprobante de reclamo: {comprobante.CodigoReclamo}");

                // En una implementación real, aquí usarías una biblioteca como iTextSharp o QuestPDF
                // Para este ejemplo, simularemos la generación del PDF

                var nombreArchivo = $"Comprobante_Entrega_{comprobante.CodigoReclamo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var rutaArchivo = $"/Documents/entrega/{nombreArchivo}";

                // Simulación de generación de PDF
                // En producción, aquí generarías el PDF real con los datos del comprobante
                await Task.Delay(100); // Simulación de procesamiento

                _logger.LogInformation($"PDF generado en: {rutaArchivo}");
                return rutaArchivo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar PDF para comprobante: {comprobante.CodigoReclamo}");
                throw;
            }
        }

        public async Task<bool> SubirComprobanteAsync(SubirComprobanteRequest request, int personalEntregaId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Subiendo comprobante para reclamo: {request.CodigoReclamo}");

                var reclamo = await _context.Reclamos
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.ComprobanteProductoReemplazado)
                    .FirstOrDefaultAsync(r => r.CodigoReclamo == request.CodigoReclamo);

                if (reclamo == null)
                {
                    throw new InvalidOperationException($"Reclamo no encontrado: {request.CodigoReclamo}");
                }

                // Verificar que todos los productos tengan reemplazo asignado
                var productosSinReemplazo = reclamo.ReclamosProductoSns
                    .Where(rps => rps.Estado == "Aprobado" && rps.FormaCompensacion == "Reemplazo")
                    .Where(rps => rps.ComprobanteProductoReemplazado == null)
                    .ToList();

                if (productosSinReemplazo.Any())
                {
                    throw new InvalidOperationException("Todos los productos deben tener un reemplazo asignado antes de subir el comprobante");
                }

                // Guardar el PDF en el sistema de archivos
                var nombreArchivo = $"Comprobante_Firmado_{request.CodigoReclamo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var rutaArchivo = $"/Documents/entrega/firmados/{nombreArchivo}";

                // Convertir base64 a archivo (simulación)
                var pdfBytes = Convert.FromBase64String(request.PdfBase64);
                // En producción, guardarías el archivo en el sistema de archivos
                // await File.WriteAllBytesAsync(rutaArchivo, pdfBytes);

                // Crear comprobante de reemplazo
                var comprobanteReemplazo = new ComprobanteDeReemplazo
                {
                    PdfComprobanteEntregaCliente = rutaArchivo,
                    FkPersonalEntrega = personalEntregaId
                };

                _context.ComprobanteDeReemplazos.Add(comprobanteReemplazo);
                await _context.SaveChangesAsync();

                // Actualizar los comprobantes de producto reemplazado con el ID del comprobante
                var productosConReemplazo = reclamo.ReclamosProductoSns
                    .Where(rps => rps.Estado == "Aprobado" && rps.FormaCompensacion == "Reemplazo")
                    .Select(rps => rps.ComprobanteProductoReemplazado)
                    .Where(cpr => cpr != null)
                    .ToList();

                foreach (var productoReemplazado in productosConReemplazo)
                {
                    productoReemplazado!.FkComprobanteDeReemplazo = comprobanteReemplazo.Id;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Comprobante subido exitosamente para reclamo: {request.CodigoReclamo}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al subir comprobante para reclamo: {request.CodigoReclamo}");
                throw;
            }
        }

        public async Task<bool> ConfirmarEntregaAsync(ConfirmarEntregaRequest request, int personalEntregaId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Confirmando entrega para reclamo: {request.CodigoReclamo}");

                var reclamo = await _context.Reclamos
                    .Include(r => r.ReclamosProductoSns)
                    .FirstOrDefaultAsync(r => r.CodigoReclamo == request.CodigoReclamo);

                if (reclamo == null)
                {
                    throw new InvalidOperationException($"Reclamo no encontrado: {request.CodigoReclamo}");
                }

                // Obtener productos aprobados para reemplazo
                var productosAEntregar = reclamo.ReclamosProductoSns
                    .Where(rps => rps.Estado == "Aprobado" && rps.FormaCompensacion == "Reemplazo")
                    .ToList();

                if (!productosAEntregar.Any())
                {
                    throw new InvalidOperationException("No hay productos para entregar");
                }

                foreach (var producto in productosAEntregar)
                {
                    // Cambiar estado a "Compensado"
                    producto.Estado = "Compensado";

                    // Obtener el producto de reemplazo asignado
                    var productoReemplazo = await _context.ComprobanteProductoReemplazados
                        .Include(cpr => cpr.FkProductoDeReemplazoNavigation)
                        .FirstOrDefaultAsync(cpr => cpr.FkReclamosProductoSn == producto.Id);

                    if (productoReemplazo != null)
                    {
                        // Cambiar estado del producto de reemplazo a "Entregado_Como_Reemplazo_Al_Cliente"
                        productoReemplazo.FkProductoDeReemplazoNavigation.EstadoInventario = "Entregado_Como_Reemplazo_Al_Cliente";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Entrega confirmada exitosamente para reclamo: {request.CodigoReclamo}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al confirmar entrega para reclamo: {request.CodigoReclamo}");
                throw;
            }
        }

        public async Task<bool> TodosProductosTienenReemplazoAsync(string codigoReclamo)
        {
            try
            {
                var reclamo = await _context.Reclamos
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.ComprobanteProductoReemplazado)
                    .FirstOrDefaultAsync(r => r.CodigoReclamo == codigoReclamo);

                if (reclamo == null)
                {
                    return false;
                }

                var productosAEntregar = reclamo.ReclamosProductoSns
                    .Where(rps => rps.Estado == "Aprobado" && rps.FormaCompensacion == "Reemplazo")
                    .ToList();

                if (!productosAEntregar.Any())
                {
                    return false;
                }

                return productosAEntregar.All(rps => rps.ComprobanteProductoReemplazado != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar reemplazos para reclamo: {codigoReclamo}");
                throw;
            }
        }
    }
}