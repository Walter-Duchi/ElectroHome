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
                    FkPersonalEntrega = personalEntregaId
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

                // Crear nombre de archivo único
                var nombreArchivo = $"Comprobante_Entrega_{comprobante.CodigoReclamo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                // Ruta relativa para la web
                var rutaRelativaWeb = $"/Documents/entrega/{nombreArchivo}";

                // Ruta física completa
                var rutaFisica = Path.Combine(Directory.GetCurrentDirectory(), "Documents", "entrega", nombreArchivo);

                // Asegurar que el directorio existe
                var directorio = Path.GetDirectoryName(rutaFisica);
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                // Generar contenido HTML del comprobante
                var htmlContent = GenerarHtmlComprobante(comprobante);

                // En una implementación real, usarías una librería como iTextSharp o QuestPDF
                // Para este ejemplo, crearé un PDF básico usando DinkToPdf o similar
                await GenerarPdfDesdeHtml(htmlContent, rutaFisica);

                _logger.LogInformation($"PDF generado en: {rutaFisica}");
                return rutaRelativaWeb;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar PDF para comprobante: {comprobante.CodigoReclamo}");
                throw;
            }
        }

        private string GenerarHtmlComprobante(ComprobanteEntregaDTO comprobante)
        {
            return $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <title>Comprobante de Entrega - {comprobante.CodigoReclamo}</title>
        <style>
            body {{ font-family: Arial, sans-serif; margin: 40px; }}
            .header {{ text-align: center; border-bottom: 2px solid #333; padding-bottom: 20px; }}
            .title {{ font-size: 24px; font-weight: bold; }}
            .info {{ margin-top: 30px; }}
            .info-row {{ margin: 10px 0; }}
            .info-label {{ font-weight: bold; display: inline-block; width: 200px; }}
            .table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
            .table th, .table td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
            .table th {{ background-color: #f2f2f2; }}
            .footer {{ margin-top: 50px; text-align: center; }}
            .signature {{ margin-top: 100px; border-top: 1px solid #333; padding-top: 20px; }}
        </style>
    </head>
    <body>
        <div class='header'>
            <div class='title'>COMPROBANTE DE ENTREGA</div>
            <div>Sistema de Gestión de Reclamos</div>
        </div>
        
        <div class='info'>
            <div class='info-row'>
                <span class='info-label'>Código de Reclamo:</span>
                <span>{comprobante.CodigoReclamo}</span>
            </div>
            <div class='info-row'>
                <span class='info-label'>Cliente:</span>
                <span>{comprobante.Cliente}</span>
            </div>
            <div class='info-row'>
                <span class='info-label'>RUC:</span>
                <span>{comprobante.Ruc}</span>
            </div>
            <div class='info-row'>
                <span class='info-label'>Fecha de Entrega:</span>
                <span>{comprobante.FechaEntrega:dd/MM/yyyy HH:mm}</span>
            </div>
            <div class='info-row'>
                <span class='info-label'>Personal de Entrega:</span>
                <span>{comprobante.PersonalEntrega}</span>
            </div>
        </div>
        
        <h3>Productos Entregados</h3>
        <table class='table'>
            <thead>
                <tr>
                    <th>Producto Defectuoso</th>
                    <th>Marca</th>
                    <th>Modelo</th>
                    <th>Producto de Reemplazo</th>
                </tr>
            </thead>
            <tbody>
                {string.Join("", comprobante.Productos.Select(p => $@"
                <tr>
                    <td>{p.NumeroSerieDefectuoso}</td>
                    <td>{p.Marca}</td>
                    <td>{p.Modelo}</td>
                    <td>{p.NumeroSerieReemplazo}</td>
                </tr>"))}
            </tbody>
        </table>
        
        <div class='footer'>
            <div class='signature'>
                <p>___________________________________</p>
                <p>Firma del Cliente</p>
                <p>Nombre: ___________________________</p>
                <p>Cédula/RUC: ______________________</p>
                <p>Fecha: ____________________________</p>
            </div>
        </div>
    </body>
    </html>";
        }

        private async Task GenerarPdfDesdeHtml(string htmlContent, string rutaSalida)
        {
            try
            {
                // Opción 1: Usar una librería como DinkToPdf (instala el paquete NuGet)
                // Opción 2: Usar PuppeteerSharp (instala el paquete NuGet)
                // Opción 3: Para desarrollo, crear un archivo HTML temporal

                // Para solución temporal, crear un archivo HTML que puedas convertir manualmente
                var htmlPath = rutaSalida.Replace(".pdf", ".html");
                await File.WriteAllTextAsync(htmlPath, htmlContent);

                _logger.LogInformation($"HTML generado en: {htmlPath}. Para producción, instala una librería de generación de PDFs.");

                // En producción, descomenta y configura una librería de PDF:
                // using var converter = new BasicConverter(new PdfTools());
                // var doc = new HtmlToPdfDocument { GlobalSettings, Objects };
                // var pdf = converter.Convert(doc);
                // await File.WriteAllBytesAsync(rutaSalida, pdf);

                // Para desarrollo, copiar un PDF de ejemplo
                var pdfEjemplo = Path.Combine(Directory.GetCurrentDirectory(), "Documents", "reclamos", "REC-20251231-2C73C3D0.pdf");
                if (File.Exists(pdfEjemplo))
                {
                    File.Copy(pdfEjemplo, rutaSalida, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF desde HTML");
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

                // Guardar el PDF en el sistema de archivos
                var nombreArchivo = $"Comprobante_Firmado_{request.CodigoReclamo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var rutaArchivo = $"/Documents/entrega/firmados/{nombreArchivo}";

                // Actualizar el comprobante existente con el PDF firmado
                comprobanteExistente.PdfComprobanteEntregaCliente = rutaArchivo;

                // Actualizar el PDF de cada producto reclamado si es necesario
                foreach (var productoReclamado in reclamo.ReclamosProductoSns)
                {
                    if (productoReclamado.ComprobanteProductoReemplazado != null)
                    {
                        // Asegurar que todos apunten al mismo comprobante
                        productoReclamado.ComprobanteProductoReemplazado.FkComprobanteDeReemplazo = comprobanteExistente.Id;
                    }
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