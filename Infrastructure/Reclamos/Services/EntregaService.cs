using Application.DTOs.Reclamos.Entrega;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reclamos.Services
{
    public class EntregaService : IEntregaService
    {
        private readonly ReclamosContext _context;
        private readonly ILogger<EntregaService> _logger;
        private readonly string _entregaDocumentsPath;

        public EntregaService(ReclamosContext context, ILogger<EntregaService> logger)
        {
            _context = context;
            _logger = logger;

            // Definir la ruta donde se guardarán los PDFs
            _entregaDocumentsPath = Path.Combine(Directory.GetCurrentDirectory(), "Documents", "entrega");
            if (!Directory.Exists(_entregaDocumentsPath))
            {
                Directory.CreateDirectory(_entregaDocumentsPath);
            }
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

                // VERIFICACIÓN CRÍTICA: Si no hay productos para entregar, NO permitir continuar
                if (!productosParaEntrega.Any())
                {
                    return new BuscarReclamoResponse
                    {
                        Exito = false,
                        Mensaje = "No hay productos para entregar en este reclamo. Los productos deben estar aprobados y con forma de compensación 'Reemplazo'.",
                        CodigoReclamo = reclamo.CodigoReclamo,
                        Cliente = $"{reclamo.FkEmpresaClienteNavigation.Nombres} {reclamo.FkEmpresaClienteNavigation.Apellidos}",
                        Ruc = reclamo.FkEmpresaClienteNavigation.Ruc,
                        Productos = new List<ProductoEntregaDTO>(),
                        TodosProductosRevisados = productosPendientesRevision == 0,
                        TotalProductosReclamo = todosProductos.Count,
                        ProductosPendientesRevision = productosPendientesRevision
                    };
                }

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
                    .Include(cpr => cpr.FkReclamosProductoSnNavigation)
                    .AnyAsync(cpr => cpr.FkProductoDeReemplazo == productoReemplazo.Id
                    && cpr.FkReclamosProductoSn != reclamoProductoSnId);

                if (yaAsignado)
                {
                    return new ValidarReemplazoResponse
                    {
                        Valido = false,
                        Mensaje = "El producto de reemplazo ya ha sido asignado a otro reclamo"
                    };
                }

                // Verificar que no sea el mismo producto defectuoso
                if (productoReclamado.FkNumeroSerieProductos == productoReemplazo.Id)
                {
                    return new ValidarReemplazoResponse
                    {
                        Valido = false,
                        Mensaje = "No puede asignar el mismo producto defectuoso como reemplazo"
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

                // Crear un ComprobanteDeReemplazo temporal con estado pendiente
                var comprobanteReemplazo = new ComprobanteDeReemplazo
                {
                    PdfComprobanteEntregaCliente = "PENDIENTE_" + Guid.NewGuid().ToString(),
                    FkPersonalEntrega = personalEntregaId,
                    Estado = "Pendiente"
                };

                _context.ComprobanteDeReemplazos.Add(comprobanteReemplazo);
                await _context.SaveChangesAsync();

                // Crear la relación en ComprobanteProductoReemplazado
                var comprobanteProducto = new ComprobanteProductoReemplazado
                {
                    FkReclamosProductoSn = request.ReclamoProductoSnId,
                    FkProductoDeReemplazo = validacion.ProductoReemplazo!.Id,
                    FkComprobanteDeReemplazo = comprobanteReemplazo.Id
                };

                _context.ComprobanteProductoReemplazados.Add(comprobanteProducto);
                await _context.SaveChangesAsync();

                // Actualizar el estado del producto de reemplazo a "Entregado_Como_Reemplazo_Al_Cliente"
                var productoReemplazo = await _context.NumeroSerieProductos
                    .FindAsync(validacion.ProductoReemplazo.Id);

                if (productoReemplazo != null)
                {
                    productoReemplazo.EstadoInventario = "Entregado_Como_Reemplazo_Al_Cliente";
                    await _context.SaveChangesAsync();
                }

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

                // VERIFICACIÓN CRÍTICA: Si no hay productos para entregar, lanzar excepción
                var productosAEntregar = reclamo.ReclamosProductoSns
                    .Where(rps => rps.Estado == "Aprobado" && rps.FormaCompensacion == "Reemplazo")
                    .ToList();

                if (!productosAEntregar.Any())
                {
                    throw new InvalidOperationException("No hay productos para entregar. No se puede generar el comprobante.");
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
                    FirmaBase64 = string.Empty
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

                // VERIFICACIÓN CRÍTICA: Si no hay productos, no generar PDF
                if (!comprobante.Productos.Any())
                {
                    throw new InvalidOperationException("No hay productos para incluir en el comprobante. No se puede generar el PDF.");
                }

                // Crear nombre de archivo único basado en timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var nombreArchivo = $"Comprobante_Entrega_{comprobante.CodigoReclamo}_{timestamp}.pdf";
                var rutaCompleta = Path.Combine(_entregaDocumentsPath, nombreArchivo);

                // Generar PDF usando QuestPDF
                var document = new ComprobanteEntregaDocument(comprobante);
                document.GeneratePdf(rutaCompleta);

                _logger.LogInformation($"PDF generado exitosamente en: {rutaCompleta}");

                // Retornar ruta relativa para la web
                return $"/Documents/entrega/{nombreArchivo}";
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

                // Verificar que el base64 no esté vacío
                if (string.IsNullOrWhiteSpace(request.PdfBase64))
                {
                    throw new InvalidOperationException("El PDF firmado es requerido");
                }

                var reclamo = await _context.Reclamos
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.ComprobanteProductoReemplazado)
                            .ThenInclude(cpr => cpr.FkComprobanteDeReemplazoNavigation)
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

                // Obtener el comprobante existente (creado al asignar el reemplazo)
                var comprobanteExistente = reclamo.ReclamosProductoSns
                    .Where(rps => rps.ComprobanteProductoReemplazado != null)
                    .Select(rps => rps.ComprobanteProductoReemplazado.FkComprobanteDeReemplazoNavigation)
                    .FirstOrDefault();

                if (comprobanteExistente == null)
                {
                    throw new InvalidOperationException("No se encontró comprobante para actualizar");
                }

                // Convertir base64 a bytes
                var pdfBytes = Convert.FromBase64String(request.PdfBase64);

                // Crear nombre único para el archivo
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var nombreArchivo = $"Comprobante_Firmado_{request.CodigoReclamo}_{timestamp}.pdf";
                var rutaCompleta = Path.Combine(_entregaDocumentsPath, nombreArchivo);

                // Guardar el archivo
                await File.WriteAllBytesAsync(rutaCompleta, pdfBytes);

                // Actualizar el comprobante existente con el PDF firmado
                comprobanteExistente.PdfComprobanteEntregaCliente = $"/Documents/entrega/{nombreArchivo}";
                comprobanteExistente.Estado = "Firmado";

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
                        .ThenInclude(rps => rps.ComprobanteProductoReemplazado)
                            .ThenInclude(cpr => cpr.FkComprobanteDeReemplazoNavigation)
                    .FirstOrDefaultAsync(r => r.CodigoReclamo == request.CodigoReclamo);

                if (reclamo == null)
                {
                    throw new InvalidOperationException($"Reclamo no encontrado: {request.CodigoReclamo}");
                }

                // Verificar que el comprobante esté firmado
                var comprobante = reclamo.ReclamosProductoSns
                    .Where(rps => rps.ComprobanteProductoReemplazado != null)
                    .Select(rps => rps.ComprobanteProductoReemplazado.FkComprobanteDeReemplazoNavigation)
                    .FirstOrDefault();

                if (comprobante == null || comprobante.Estado != "Firmado")
                {
                    throw new InvalidOperationException("El comprobante debe estar firmado antes de confirmar la entrega");
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

                // Actualizar estado del comprobante
                if (comprobante != null)
                {
                    comprobante.Estado = "Completado";
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

    // Clase interna para generar el documento PDF con QuestPDF
    internal class ComprobanteEntregaDocument : IDocument
    {
        private readonly ComprobanteEntregaDTO _comprobante;

        public ComprobanteEntregaDocument(ComprobanteEntregaDTO comprobante)
        {
            _comprobante = comprobante;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                // ENCABEZADO
                page.Header()
                    .Column(col =>
                    {
                        col.Item()
                            .Text("COMPROBANTE DE ENTREGA")
                            .Bold()
                            .FontSize(20)
                            .AlignCenter()
                            .FontColor(Colors.Black);

                        col.Item()
                            .PaddingBottom(20)
                            .Text("Sistema de Gestión de Reclamos")
                            .FontSize(12)
                            .AlignCenter()
                            .FontColor(Colors.Grey.Darken1);
                    });

                // CONTENIDO
                page.Content()
                    .PaddingVertical(10)
                    .Column(col =>
                    {
                        // INFORMACIÓN DEL RECLAMO
                        col.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(3);
                                });

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("Código de Reclamo:").Bold();
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text(_comprobante.CodigoReclamo);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("Cliente:").Bold();
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text(_comprobante.Cliente);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("RUC:").Bold();
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text(_comprobante.Ruc);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("Fecha de Entrega:").Bold();
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text(_comprobante.FechaEntrega.ToString("dd/MM/yyyy HH:mm"));

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("Personal de Entrega:").Bold();
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text(_comprobante.PersonalEntrega);
                            });

                        col.Item().PaddingVertical(20);

                        // TABLA DE PRODUCTOS
                        col.Item()
                            .PaddingBottom(10)
                            .Element(e =>
                            {
                                e.Text("Productos Entregados")
                                 .Bold()
                                 .FontSize(14);
                            });

                        col.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // Producto Defectuoso
                                    columns.RelativeColumn(); // Marca
                                    columns.RelativeColumn(); // Modelo
                                    columns.RelativeColumn(); // Producto de Reemplazo
                                });

                                // ENCABEZADOS DE LA TABLA
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Text("Producto Defectuoso").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Text("Marca").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Text("Modelo").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Text("Producto de Reemplazo").Bold();
                                });

                                // FILAS DE PRODUCTOS
                                foreach (var producto in _comprobante.Productos)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(producto.NumeroSerieDefectuoso);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(producto.Marca);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(producto.Modelo);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(producto.NumeroSerieReemplazo);
                                }
                            });

                        col.Item().PaddingVertical(40);

                        // FIRMA DEL CLIENTE
                        col.Item()
                            .BorderTop(1)
                            .BorderColor(Colors.Black)
                            .PaddingTop(20)
                            .Column(firmaCol =>
                            {
                                firmaCol.Item()
                                    .Text("___________________________________")
                                    .AlignCenter();

                                firmaCol.Item()
                                    .PaddingTop(5)
                                    .Text("Firma del Cliente")
                                    .AlignCenter();
                            });
                    });

                // PIE DE PÁGINA
                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Generado el ").FontSize(9);
                        text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9);
                    });
            });
        }
    }
}