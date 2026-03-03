using Application.DTOs.Reclamos.Reclamo;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reclamos.Services
{
    public class ReclamoService : IReclamoService
    {
        private readonly ReclamosContext _context;
        private readonly ILogger<ReclamoService> _logger;
        private readonly IConfiguration _configuration;

        public ReclamoService(ReclamosContext context, ILogger<ReclamoService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<ValidarClienteResponse> ValidarClienteAsync(string ruc)
        {
            try
            {
                var cliente = await _context.Usuarios
                    .Where(u => u.Ruc == ruc && u.Rol == "Cliente")
                    .Select(u => new { u.Id, u.Nombres, u.Apellidos, u.Ruc })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return new ValidarClienteResponse
                    {
                        EsValido = false,
                        Mensaje = "Cliente no encontrado o no tiene rol de cliente."
                    };
                }

                return new ValidarClienteResponse
                {
                    EsValido = true,
                    Mensaje = "Cliente válido.",
                    ClienteId = cliente.Id,
                    RazonSocial = $"{cliente.Nombres} {cliente.Apellidos}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar cliente con RUC: {Ruc}", ruc);
                return new ValidarClienteResponse
                {
                    EsValido = false,
                    Mensaje = "Error interno al validar cliente."
                };
            }
        }

        public async Task<ValidarProductoResponse> ValidarProductoAsync(string numeroSerie)
        {
            try
            {
                var producto = await _context.NumeroSerieProductos
                    .Include(nsp => nsp.FkProductoNavigation)
                        .ThenInclude(p => p.FkMarcaNavigation)
                    .Include(nsp => nsp.VentasPorNumeroSerieProducto)
                        .ThenInclude(vpsp => vpsp.FkVentasNavigation)
                    .FirstOrDefaultAsync(nsp => nsp.NumeroSerie == numeroSerie);

                if (producto == null)
                {
                    return new ValidarProductoResponse
                    {
                        EsValido = false,
                        Mensaje = "Producto no encontrado."
                    };
                }

                if (producto.EstadoInventario != "Vendido" && producto.EstadoInventario != "Entregado_Como_Reemplazo_Al_Cliente")
                {
                    return new ValidarProductoResponse
                    {
                        EsValido = false,
                        Mensaje = $"El producto no está disponible para reclamar. Estado: {producto.EstadoInventario}"
                    };
                }

                DateTime? fechaVenta = null;
                if (producto.VentasPorNumeroSerieProducto != null)
                {
                    fechaVenta = producto.VentasPorNumeroSerieProducto.FkVentasNavigation?.FechaCompra;
                }

                bool tieneGarantia = false;
                int? diasGarantia = producto.FkProductoNavigation?.DiasGarantia;

                if (diasGarantia == 0)
                {
                    return new ValidarProductoResponse
                    {
                        EsValido = false,
                        Mensaje = "Producto descontinuado, no tiene garantía.",
                        ProductoId = producto.Id,
                        Marca = producto.FkProductoNavigation?.FkMarcaNavigation?.Nombre,
                        Modelo = producto.FkProductoNavigation?.Modelo,
                        TieneGarantia = false,
                        EstadoInventario = producto.EstadoInventario,
                        FechaVenta = fechaVenta,
                        DiasGarantia = diasGarantia,
                        Especificacion = producto.FkProductoNavigation?.Especificacion,
                        Precio = producto.FkProductoNavigation?.Precio
                    };
                }

                if (fechaVenta.HasValue && diasGarantia.HasValue)
                {
                    var fechaLimiteGarantia = fechaVenta.Value.AddDays(diasGarantia.Value);
                    if (fechaLimiteGarantia >= DateTime.Now)
                    {
                        tieneGarantia = true;
                    }
                }

                if (!tieneGarantia)
                {
                    return new ValidarProductoResponse
                    {
                        EsValido = false,
                        Mensaje = "El producto ha excedido el período de garantía.",
                        ProductoId = producto.Id,
                        Marca = producto.FkProductoNavigation?.FkMarcaNavigation?.Nombre,
                        Modelo = producto.FkProductoNavigation?.Modelo,
                        TieneGarantia = false,
                        EstadoInventario = producto.EstadoInventario,
                        FechaVenta = fechaVenta,
                        DiasGarantia = diasGarantia,
                        Especificacion = producto.FkProductoNavigation?.Especificacion,
                        Precio = producto.FkProductoNavigation?.Precio
                    };
                }

                return new ValidarProductoResponse
                {
                    EsValido = true,
                    Mensaje = "Producto válido para reclamo.",
                    ProductoId = producto.Id,
                    Marca = producto.FkProductoNavigation?.FkMarcaNavigation?.Nombre,
                    Modelo = producto.FkProductoNavigation?.Modelo,
                    TieneGarantia = true,
                    EstadoInventario = producto.EstadoInventario,
                    FechaVenta = fechaVenta,
                    DiasGarantia = diasGarantia,
                    Especificacion = producto.FkProductoNavigation?.Especificacion,
                    Precio = producto.FkProductoNavigation?.Precio
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar producto con número de serie: {NumeroSerie}", numeroSerie);
                return new ValidarProductoResponse
                {
                    EsValido = false,
                    Mensaje = "Error interno al validar producto."
                };
            }
        }

        public async Task<CrearReclamoResponse> CrearReclamoAsync(CrearReclamoRequest request, int revisorId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("========================================");
                _logger.LogInformation("INICIANDO CREACIÓN DE RECLAMO CON DISTRIBUCIÓN EQUITATIVA DE TÉCNICOS");
                _logger.LogInformation("RevisorId: {RevisorId}", revisorId);
                _logger.LogInformation("RUC Cliente: {RucCliente}", request.RucCliente);
                _logger.LogInformation("Productos solicitados: {@Productos}", request.Productos);
                _logger.LogInformation("========================================");

                // 1. Validar cliente
                _logger.LogInformation("Validando cliente con RUC: {RucCliente}", request.RucCliente);
                var cliente = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Ruc == request.RucCliente && u.Rol == "Cliente");

                if (cliente == null)
                {
                    _logger.LogError("Cliente no encontrado con RUC: {RucCliente}", request.RucCliente);
                    return new CrearReclamoResponse
                    {
                        Exito = false,
                        Mensaje = "Cliente no válido."
                    };
                }
                _logger.LogInformation("Cliente validado: {ClienteId} - {Nombre}", cliente.Id, $"{cliente.Nombres} {cliente.Apellidos}");

                // 2. Validar cada producto y obtener información de marcas
                var productosConMarca = new List<(int numeroSerieProductoId, string formaCompensacion, string numeroSerie, int marcaId)>();

                foreach (var productoReq in request.Productos)
                {
                    _logger.LogInformation("Validando producto: {NumeroSerie}", productoReq.NumeroSerie);

                    // Primero, verificar si el producto ya está en un reclamo
                    var productoExistente = await _context.NumeroSerieProductos
                        .Include(nsp => nsp.FkProductoNavigation)
                        .FirstOrDefaultAsync(nsp => nsp.NumeroSerie == productoReq.NumeroSerie);

                    if (productoExistente == null)
                    {
                        _logger.LogError("Producto no encontrado: {NumeroSerie}", productoReq.NumeroSerie);
                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = $"Producto {productoReq.NumeroSerie} no encontrado."
                        };
                    }

                    // Verificar si ya está en un reclamo activo
                    var yaEnReclamo = await _context.ReclamosProductoSns
                        .AnyAsync(rps => rps.FkNumeroSerieProductos == productoExistente.Id &&
                        (rps.Estado == "Pendiente" || rps.Estado == "En Revision" || rps.Estado == "Aprobado"));

                    if (yaEnReclamo)
                    {
                        _logger.LogError("Producto {NumeroSerie} ya está en un reclamo activo", productoReq.NumeroSerie);
                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = $"El producto {productoReq.NumeroSerie} ya está en un reclamo activo y no puede ser reclamado nuevamente."
                        };
                    }

                    var validacion = await ValidarProductoAsync(productoReq.NumeroSerie);

                    _logger.LogInformation("Resultado validación producto {NumeroSerie}: EsValido={EsValido}, TieneGarantia={TieneGarantia}, Mensaje={Mensaje}",
                        productoReq.NumeroSerie, validacion.EsValido, validacion.TieneGarantia, validacion.Mensaje);

                    if (!validacion.EsValido || !validacion.TieneGarantia)
                    {
                        _logger.LogError("Producto no válido: {NumeroSerie} - {Mensaje}",
                            productoReq.NumeroSerie, validacion.Mensaje);

                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = $"Producto {productoReq.NumeroSerie} no válido: {validacion.Mensaje}"
                        };
                    }

                    if (!validacion.ProductoId.HasValue)
                    {
                        _logger.LogError("ProductoId es null para: {NumeroSerie}", productoReq.NumeroSerie);
                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = $"Error interno: ProductoId no encontrado para {productoReq.NumeroSerie}"
                        };
                    }

                    // Obtener marca del producto para asignación de técnico
                    var marcaId = await _context.NumeroSerieProductos
                        .Where(nsp => nsp.Id == validacion.ProductoId.Value)
                        .Select(nsp => nsp.FkProductoNavigation.FkMarca)
                        .FirstOrDefaultAsync();

                    productosConMarca.Add((validacion.ProductoId.Value, productoReq.FormaCompensacion, productoReq.NumeroSerie, marcaId));
                }

                // 3. Crear código de reclamo
                var codigoReclamo = $"REC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                _logger.LogInformation("Código de reclamo generado: {CodigoReclamo}", codigoReclamo);

                // 4. Crear entidad Reclamo
                var reclamo = new Reclamo
                {
                    CodigoReclamo = codigoReclamo,
                    FkEmpresaCliente = cliente.Id,
                    FechaCreacionReclamo = DateTime.Now
                };

                _context.Reclamos.Add(reclamo);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Reclamo creado en BD con ID: {ReclamoId}", reclamo.Id);

                // 5. ASIGNACIÓN EQUITATIVA DE TÉCNICOS - PRODUCTO POR PRODUCTO
                _logger.LogInformation("========================================");
                _logger.LogInformation("INICIANDO ASIGNACIÓN EQUITATIVA DE TÉCNICOS");
                _logger.LogInformation("Total productos a asignar: {CantidadProductos}", productosConMarca.Count);
                _logger.LogInformation("========================================");

                // Obtener todos los técnicos certificados por marca
                var certificacionesPorMarca = await _context.UsuariosCertificacionMarcas
                    .Include(ucm => ucm.FkTecnicoNavigation)
                    .Where(ucm => ucm.FkTecnicoNavigation.Rol == "Tecnico")
                    .GroupBy(ucm => ucm.FkMarca)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(ucm => ucm.FkTecnicoNavigation).ToList()
                    );

                // Contador para distribución round-robin por marca
                var indiceTecnicoPorMarca = new Dictionary<int, int>();

                // Obtener carga actual de trabajo para cada técnico
                var cargaTrabajo = await _context.ReclamosProductoSns
                    .Where(rps => rps.FkTecnicoAsignado != null && (rps.Estado == "Pendiente" || rps.Estado == "En Revision"))
                    .GroupBy(rps => rps.FkTecnicoAsignado)
                    .Select(g => new { TecnicoId = g.Key, Carga = g.Count() })
                    .ToDictionaryAsync(x => x.TecnicoId, x => x.Carga);

                // Lista para guardar asignaciones finales
                var productosConTecnico = new List<(int numeroSerieProductoId, string formaCompensacion, string numeroSerie, int marcaId, Usuario tecnicoAsignado)>();

                // Procesar cada producto uno por uno
                foreach (var (numeroSerieProductoId, formaCompensacion, numeroSerie, marcaId) in productosConMarca)
                {
                    _logger.LogInformation("Procesando producto {NumeroSerie} de marca {MarcaId}", numeroSerie, marcaId);

                    // Verificar si hay técnicos certificados para esta marca
                    if (!certificacionesPorMarca.ContainsKey(marcaId) || !certificacionesPorMarca[marcaId].Any())
                    {
                        _logger.LogError("CRÍTICO: No hay técnicos certificados para la marca {MarcaId}", marcaId);
                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = $"No hay técnicos certificados para la marca del producto {numeroSerie}. El reclamo no puede ser creado."
                        };
                    }

                    // Obtener técnicos certificados para esta marca
                    var tecnicosCertificados = certificacionesPorMarca[marcaId];

                    // Asignar técnico usando algoritmo round-robin con balance de carga
                    Usuario tecnicoAsignado = null;

                    // Primero intentamos asignar al técnico con menor carga
                    var tecnicosOrdenadosPorCarga = tecnicosCertificados
                        .OrderBy(t => cargaTrabajo.GetValueOrDefault(t.Id, 0))
                        .ToList();

                    tecnicoAsignado = tecnicosOrdenadosPorCarga.First();

                    // Si hay empate en carga, usamos round-robin
                    var cargaMinima = cargaTrabajo.GetValueOrDefault(tecnicosOrdenadosPorCarga.First().Id, 0);
                    var tecnicosConCargaMinima = tecnicosOrdenadosPorCarga
                        .Where(t => cargaTrabajo.GetValueOrDefault(t.Id, 0) == cargaMinima)
                        .ToList();

                    if (tecnicosConCargaMinima.Count > 1)
                    {
                        // Usar round-robin para desempatar
                        if (!indiceTecnicoPorMarca.ContainsKey(marcaId))
                        {
                            indiceTecnicoPorMarca[marcaId] = 0;
                        }

                        var indice = indiceTecnicoPorMarca[marcaId] % tecnicosConCargaMinima.Count;
                        tecnicoAsignado = tecnicosConCargaMinima[indice];
                        indiceTecnicoPorMarca[marcaId] = (indice + 1) % tecnicosConCargaMinima.Count;
                    }

                    // Verificar certificación del técnico (ya está garantizado por la consulta)
                    var tieneCertificacion = await _context.UsuariosCertificacionMarcas
                        .AnyAsync(ucm => ucm.FkMarca == marcaId && ucm.FkTecnico == tecnicoAsignado.Id);

                    if (!tieneCertificacion)
                    {
                        _logger.LogError("CRÍTICO: El técnico {TecnicoId} no está certificado para la marca {MarcaId}",
                            tecnicoAsignado.Id, marcaId);
                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = "Error de certificación: Técnico asignado no tiene certificación válida."
                        };
                    }

                    // Actualizar carga de trabajo para el próximo producto
                    if (cargaTrabajo.ContainsKey(tecnicoAsignado.Id))
                    {
                        cargaTrabajo[tecnicoAsignado.Id]++;
                    }
                    else
                    {
                        cargaTrabajo[tecnicoAsignado.Id] = 1;
                    }

                    productosConTecnico.Add((numeroSerieProductoId, formaCompensacion, numeroSerie, marcaId, tecnicoAsignado));

                    _logger.LogInformation("Asignado técnico {TecnicoId} ({Nombre}) al producto {NumeroSerie}. Carga actual: {Carga}",
                        tecnicoAsignado.Id, $"{tecnicoAsignado.Nombres} {tecnicoAsignado.Apellidos}",
                        numeroSerie, cargaTrabajo[tecnicoAsignado.Id]);
                }

                // 6. Verificar que TODOS los productos tengan técnico asignado (esto ya está garantizado por la lógica anterior)
                if (productosConTecnico.Any(p => p.tecnicoAsignado == null))
                {
                    _logger.LogError("CRÍTICO: Uno o más productos no tienen técnico asignado");
                    await transaction.RollbackAsync();
                    return new CrearReclamoResponse
                    {
                        Exito = false,
                        Mensaje = "No se pudieron asignar técnicos certificados para todos los productos. El reclamo no puede ser creado."
                    };
                }

                _logger.LogInformation("========================================");
                _logger.LogInformation("ASIGNACIONES COMPLETADAS - RESUMEN");
                _logger.LogInformation("Total productos: {TotalProductos}", productosConTecnico.Count);

                // Log distribución de carga final
                foreach (var carga in cargaTrabajo)
                {
                    var tecnico = await _context.Usuarios.FindAsync(carga.Key);
                    if (tecnico != null)
                    {
                        _logger.LogInformation("Técnico {TecnicoId} ({Nombre}): {Carga} productos asignados",
                            tecnico.Id, $"{tecnico.Nombres} {tecnico.Apellidos}", carga.Value);
                    }
                }
                _logger.LogInformation("========================================");

                // 7. Crear productos del reclamo CON técnicos ya asignados
                var reclamosProductos = new List<ReclamosProductoSn>();

                foreach (var (numeroSerieProductoId, formaCompensacion, numeroSerie, marcaId, tecnicoAsignado) in productosConTecnico)
                {
                    if (tecnicoAsignado == null)
                    {
                        _logger.LogError("CRÍTICO: Técnico null encontrado para producto {ProductoId}", numeroSerieProductoId);
                        await transaction.RollbackAsync();
                        return new CrearReclamoResponse
                        {
                            Exito = false,
                            Mensaje = "Error crítico: Técnico no asignado para un producto."
                        };
                    }

                    var reclamoProducto = new ReclamosProductoSn
                    {
                        FkNumeroSerieProductos = numeroSerieProductoId,
                        FkReclamos = reclamo.Id,
                        FormaCompensacion = formaCompensacion,
                        Estado = "Pendiente",
                        FkTecnicoAsignado = tecnicoAsignado.Id,
                        FechaReclamoClienteFinal = DateTime.Now
                    };

                    reclamosProductos.Add(reclamoProducto);
                    _context.ReclamosProductoSns.Add(reclamoProducto);

                    _logger.LogInformation("Producto {NumeroSerie} agregado al reclamo con técnico {TecnicoId} ({Nombre}) certificado en marca {MarcaId}",
                        numeroSerie, tecnicoAsignado.Id, $"{tecnicoAsignado.Nombres} {tecnicoAsignado.Apellidos}", marcaId);
                }

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Todos los productos agregados al reclamo con técnicos asignados");
                }
                catch (DbUpdateException dbEx)
                {
                    var innerException = dbEx.InnerException;
                    while (innerException != null)
                    {
                        _logger.LogError("Inner Exception: {Message}", innerException.Message);
                        innerException = innerException.InnerException;
                    }

                    await transaction.RollbackAsync();
                    return new CrearReclamoResponse
                    {
                        Exito = false,
                        Mensaje = $"Error de base de datos al guardar el reclamo: {dbEx.InnerException?.Message ?? dbEx.Message}"
                    };
                }

                // 8. Generar PDF REAL (no HTML)
                _logger.LogInformation("Generando PDF REAL para reclamo {ReclamoId}", reclamo.Id);
                var pdfResult = await GenerarPdfRealAsync(reclamo.Id, codigoReclamo, cliente, productosConTecnico);

                if (!pdfResult.exito)
                {
                    _logger.LogError("Error al generar PDF para reclamo {ReclamoId}: {Error}", reclamo.Id, pdfResult.mensajeError);
                    await transaction.RollbackAsync();
                    return new CrearReclamoResponse
                    {
                        Exito = false,
                        Mensaje = $"Error al generar el comprobante PDF: {pdfResult.mensajeError}"
                    };
                }

                _logger.LogInformation("PDF generado correctamente y guardado en: {RutaPdf}", pdfResult.rutaArchivo);

                // 9. Commit de transacción
                await transaction.CommitAsync();
                _logger.LogInformation("========================================");
                _logger.LogInformation("TRANSACCIÓN COMPLETADA EXITOSAMENTE");
                _logger.LogInformation("Reclamo {ReclamoId} creado con {Cantidad} productos", reclamo.Id, productosConTecnico.Count);
                _logger.LogInformation("Todos los productos tienen técnico asignado");
                _logger.LogInformation("Cargas distribuidas equitativamente entre técnicos");
                _logger.LogInformation("========================================");

                return new CrearReclamoResponse
                {
                    Exito = true,
                    Mensaje = "Reclamo creado exitosamente con asignación equitativa de técnicos.",
                    ReclamoId = reclamo.Id,
                    CodigoReclamo = codigoReclamo,
                    PdfBase64 = pdfResult.pdfBase64,
                    PdfFileName = $"{codigoReclamo}.pdf"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear reclamo para el cliente con RUC: {Ruc}", request.RucCliente);

                var errorMessage = ex.Message;
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    errorMessage += $" -> {innerEx.Message}";
                    innerEx = innerEx.InnerException;
                }

                return new CrearReclamoResponse
                {
                    Exito = false,
                    Mensaje = $"Error interno al crear el reclamo: {errorMessage}"
                };
            }
        }

        private async Task<(bool exito, string pdfBase64, string rutaArchivo, string mensajeError)> GenerarPdfRealAsync(
            int reclamoId, string codigoReclamo, Usuario cliente,
            List<(int numeroSerieProductoId, string formaCompensacion, string numeroSerie, int marcaId, Usuario tecnicoAsignado)> productos)
        {
            try
            {
                // Obtener detalles completos de los productos
                var productosDetalle = new List<dynamic>();
                foreach (var (id, forma, numeroSerie, marcaId, tecnico) in productos)
                {
                    var producto = await _context.NumeroSerieProductos
                        .Include(nsp => nsp.FkProductoNavigation)
                            .ThenInclude(p => p.FkMarcaNavigation)
                        .FirstOrDefaultAsync(nsp => nsp.Id == id);

                    if (producto != null)
                    {
                        productosDetalle.Add(new
                        {
                            NumeroSerie = numeroSerie,
                            Marca = producto.FkProductoNavigation?.FkMarcaNavigation?.Nombre,
                            Modelo = producto.FkProductoNavigation?.Modelo,
                            Especificacion = producto.FkProductoNavigation?.Especificacion,
                            FormaCompensacion = forma,
                            Precio = producto.FkProductoNavigation?.Precio,
                            TecnicoAsignado = tecnico != null ? $"{tecnico.Nombres} {tecnico.Apellidos}" : "No asignado",
                            TecnicoId = tecnico?.Id
                        });
                    }
                }

                // Generar PDF con QuestPDF
                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        page.Header()
                            .AlignCenter()
                            .Text("COMPROBANTE DE RECLAMO")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken3);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Spacing(10);

                                // Información del reclamo
                                x.Item().Text($"Código de Reclamo: {codigoReclamo}").SemiBold();
                                x.Item().Text($"Fecha de Creación: {DateTime.Now:dd/MM/yyyy HH:mm}");
                                x.Item().Text($"Reclamo ID: {reclamoId}");

                                // Información del cliente
                                x.Item().PaddingTop(10).Text("INFORMACIÓN DEL CLIENTE").SemiBold().FontSize(14);
                                x.Item().Text($"Razón Social: {cliente.Nombres} {cliente.Apellidos}");
                                x.Item().Text($"RUC: {cliente.Ruc}");
                                x.Item().Text($"Teléfono: {cliente.Celular}");

                                // Productos reclamados
                                x.Item().PaddingTop(15).Text("PRODUCTOS RECLAMADOS").SemiBold().FontSize(14);
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(100); // N° Serie
                                        columns.RelativeColumn();    // Producto
                                        columns.ConstantColumn(80);  // Precio
                                        columns.ConstantColumn(100); // Compensación
                                        columns.ConstantColumn(120); // Técnico Asignado
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("N° Serie");
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Producto");
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Precio");
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Compensación");
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Técnico");
                                    });

                                    foreach (var p in productosDetalle)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{p.NumeroSerie}");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{p.Marca} {p.Modelo}\n{p.Especificacion}");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${p.Precio:N2}");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{p.FormaCompensacion}");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{p.TecnicoAsignado}");
                                    }
                                });

                                // Resumen de distribución
                                x.Item().PaddingTop(15).Text("DISTRIBUCIÓN DE TÉCNICOS").SemiBold().FontSize(14);
                                x.Item().Text($"Total de productos: {productosDetalle.Count}");
                                x.Item().Text($"Todos los productos tienen técnico asignado: SÍ");

                                // Distribución por técnico
                                var distribucionPorTecnico = productosDetalle
                                    .GroupBy(p => p.TecnicoAsignado)
                                    .Select(g => new { Tecnico = g.Key, Cantidad = g.Count() })
                                    .ToList();

                                foreach (var dist in distribucionPorTecnico)
                                {
                                    x.Item().Text($"• {dist.Tecnico}: {dist.Cantidad} producto(s)");
                                }

                                // Observaciones
                                x.Item().PaddingTop(15).Text("OBSERVACIONES").SemiBold().FontSize(14);
                                x.Item().Text("1. Este comprobante debe ser presentado para cualquier consulta sobre el estado del reclamo.");
                                x.Item().Text("2. Los productos serán revisados por técnicos certificados en las marcas correspondientes.");
                                x.Item().Text("3. Cada producto tiene asignado un técnico específico según su marca.");
                                x.Item().Text("4. La carga de trabajo se distribuyó equitativamente entre todos los técnicos disponibles.");
                                x.Item().Text("5. El tiempo de resolución depende de la complejidad del caso.");
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Sistema de Gestión de Reclamos - Versión 1.0 | ");
                                x.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}").Italic();
                            });
                    });
                })
                .GeneratePdf();

                // Guardar PDF en la carpeta especificada
                var pdfSettings = _configuration.GetSection("PdfSettings");
                var basePath = pdfSettings["StoragePath"] ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "reclamos");

                // Crear directorio si no existe
                Directory.CreateDirectory(basePath);

                var fileName = $"{codigoReclamo}.pdf";
                var filePath = Path.Combine(basePath, fileName);

                await File.WriteAllBytesAsync(filePath, pdfBytes);
                _logger.LogInformation("PDF guardado en: {FilePath}", filePath);

                // Convertir a base64 para enviar al frontend
                var pdfBase64 = Convert.ToBase64String(pdfBytes);

                return (true, pdfBase64, filePath, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF real para reclamo {ReclamoId}", reclamoId);
                return (false, string.Empty, string.Empty, $"Error al generar PDF: {ex.Message}");
            }
        }
    }
}