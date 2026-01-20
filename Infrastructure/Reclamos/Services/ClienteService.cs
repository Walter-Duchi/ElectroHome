using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Application.DTOs.Reclamos.Cliente;
using Infrastructure.Reclamos.Interfaces;

namespace Infrastructure.Reclamos.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ReclamosContext _context;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(ReclamosContext context, ILogger<ClienteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ClienteDashboardResponse> ObtenerDashboardClienteAsync(int clienteId, ClienteDashboardRequest? filtros = null)
        {
            try
            {
                _logger.LogInformation($"Obteniendo dashboard para cliente ID: {clienteId}");

                // Consulta base de reclamos del cliente
                var query = _context.Reclamos
                    .Where(r => r.FkEmpresaCliente == clienteId)
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.FkNumeroSerieProductosNavigation)
                            .ThenInclude(nsp => nsp.FkProductoNavigation)
                                .ThenInclude(p => p.FkMarcaNavigation)
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.FkTecnicoAsignadoNavigation)
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.ReembolsoPorReclamo)
                            .ThenInclude(rpr => rpr.FkReembolsoNavigation)
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.ComprobanteProductoReemplazado)
                            .ThenInclude(cpr => cpr.FkProductoDeReemplazoNavigation)
                    .Include(r => r.ReclamosProductoSns)
                        .ThenInclude(rps => rps.ComprobanteProductoReemplazado)
                            .ThenInclude(cpr => cpr.FkComprobanteDeReemplazoNavigation)
                                .ThenInclude(cdr => cdr.FkPersonalEntregaNavigation)
                    .AsQueryable();

                // Aplicar filtros
                if (filtros != null)
                {
                    if (!string.IsNullOrEmpty(filtros.CodigoReclamo))
                        query = query.Where(r => r.CodigoReclamo.Contains(filtros.CodigoReclamo));

                    if (filtros.FechaDesde.HasValue)
                        query = query.Where(r => r.FechaCreacionReclamo >= filtros.FechaDesde.Value);

                    if (filtros.FechaHasta.HasValue)
                        query = query.Where(r => r.FechaCreacionReclamo <= filtros.FechaHasta.Value);

                    // Más filtros se aplican después en productos
                }

                var reclamos = await query.OrderByDescending(r => r.FechaCreacionReclamo).ToListAsync();

                var response = new ClienteDashboardResponse();
                var estadisticas = new EstadisticasClienteDTO();

                foreach (var reclamo in reclamos)
                {
                    var reclamoDTO = new ClienteReclamoDTO
                    {
                        ReclamoId = reclamo.Id,
                        CodigoReclamo = reclamo.CodigoReclamo,
                        FechaCreacion = (DateTime)reclamo.FechaCreacionReclamo
                    };

                    // Procesar productos del reclamo
                    foreach (var producto in reclamo.ReclamosProductoSns)
                    {
                        // Aplicar filtros a nivel de producto
                        if (!CumpleFiltrosProducto(producto, filtros))
                            continue;

                        var productoDTO = MapearProductoADTO(producto);
                        productoDTO.Prioridad = CalcularPrioridad(productoDTO);

                        reclamoDTO.Productos.Add(productoDTO);

                        // Actualizar estadísticas
                        ActualizarEstadisticas(estadisticas, productoDTO);
                    }

                    // Solo agregar reclamo si tiene productos que cumplen filtros
                    if (reclamoDTO.Productos.Any())
                    {
                        // Ordenar productos por prioridad (mayor prioridad primero)
                        reclamoDTO.Productos = reclamoDTO.Productos
                            .OrderByDescending(p => p.Prioridad)
                            .ThenByDescending(p => p.FechaReclamoClienteFinal)
                            .ToList();

                        response.Reclamos.Add(reclamoDTO);
                    }
                }

                // Ordenar reclamos por relevancia
                response.Reclamos = OrdenarReclamosPorRelevancia(response.Reclamos);
                response.Estadisticas = estadisticas;
                response.Estadisticas.TotalReclamos = response.Reclamos.Count;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener dashboard del cliente ID: {clienteId}");
                throw;
            }
        }

        private bool CumpleFiltrosProducto(Models.ReclamosProductoSn producto, ClienteDashboardRequest? filtros)
        {
            if (filtros == null) return true;

            if (!string.IsNullOrEmpty(filtros.NumeroSerie))
            {
                var numeroSerie = producto.FkNumeroSerieProductosNavigation?.NumeroSerie ?? "";
                if (!numeroSerie.Contains(filtros.NumeroSerie, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (!string.IsNullOrEmpty(filtros.TipoReclamo) && producto.FormaCompensacion != filtros.TipoReclamo)
                return false;

            if (!string.IsNullOrEmpty(filtros.Estado) && producto.Estado != filtros.Estado)
                return false;

            if (filtros.SoloPendientes.HasValue && filtros.SoloPendientes.Value && producto.Estado != "Pendiente")
                return false;

            if (filtros.SoloAprobados.HasValue && filtros.SoloAprobados.Value && producto.Estado != "Aprobado")
                return false;

            if (filtros.SoloCompensados.HasValue && filtros.SoloCompensados.Value && producto.Estado != "Compensado")
                return false;

            if (filtros.SoloReembolsos.HasValue && filtros.SoloReembolsos.Value && producto.FormaCompensacion != "Reembolso")
                return false;

            if (filtros.SoloReemplazos.HasValue && filtros.SoloReemplazos.Value && producto.FormaCompensacion != "Reemplazo")
                return false;

            return true;
        }

        private ClienteProductoDTO MapearProductoADTO(Models.ReclamosProductoSn producto)
        {
            var dto = new ClienteProductoDTO
            {
                ReclamoProductoId = producto.Id,
                Marca = producto.FkNumeroSerieProductosNavigation?.FkProductoNavigation?.FkMarcaNavigation?.Nombre ?? "",
                Modelo = producto.FkNumeroSerieProductosNavigation?.FkProductoNavigation?.Modelo ?? "",
                NumeroSerie = producto.FkNumeroSerieProductosNavigation?.NumeroSerie ?? "",
                TipoReclamo = producto.FormaCompensacion,
                Estado = producto.Estado,
                FechaVentaClienteFinal = producto.FechaVentaClienteFinal,
                FechaReclamoClienteFinal = producto.FechaReclamoClienteFinal,
                TecnicoId = producto.FkTecnicoAsignado,
                TecnicoNombre = producto.FkTecnicoAsignadoNavigation != null
                    ? $"{producto.FkTecnicoAsignadoNavigation.Nombres} {producto.FkTecnicoAsignadoNavigation.Apellidos}"
                    : null,
                FechaRevisionTecnico = producto.FechaRevisionTecnico,
                ExplicacionRespuestaTecnico = producto.ExplicacionRespuestaTecnico,
                PdfRevisionTecnico = producto.PdfRevisionTecnico
            };

            // Información de compensación
            dto.Compensacion = ObtenerInformacionCompensacion(producto);

            return dto;
        }

        private CompensacionDTO? ObtenerInformacionCompensacion(Models.ReclamosProductoSn producto)
        {
            if (producto.Estado != "Compensado") return null;

            var compensacion = new CompensacionDTO
            {
                Tipo = producto.FormaCompensacion
            };

            if (producto.FormaCompensacion == "Reembolso" && producto.ReembolsoPorReclamo != null)
            {
                var reembolso = producto.ReembolsoPorReclamo.FkReembolsoNavigation;
                compensacion.NumeroComprobanteReembolso = reembolso?.NumeroComprobanteReembolso;
                compensacion.FechaReembolso = reembolso?.FechaReembolso;
                compensacion.NumCuentaBancariaReembolso = reembolso?.NumCuentaBancariaReembolso;
            }
            else if (producto.FormaCompensacion == "Reemplazo" && producto.ComprobanteProductoReemplazado != null)
            {
                var comprobante = producto.ComprobanteProductoReemplazado;
                compensacion.NumeroSerieReemplazo = comprobante.FkProductoDeReemplazoNavigation?.NumeroSerie;
                compensacion.PdfComprobanteEntrega = comprobante.FkComprobanteDeReemplazoNavigation?.PdfComprobanteEntregaCliente;
                compensacion.PersonalEntregaNombre = comprobante.FkComprobanteDeReemplazoNavigation?.FkPersonalEntregaNavigation != null
                    ? $"{comprobante.FkComprobanteDeReemplazoNavigation.FkPersonalEntregaNavigation.Nombres} {comprobante.FkComprobanteDeReemplazoNavigation.FkPersonalEntregaNavigation.Apellidos}"
                    : null;
            }

            return compensacion;
        }

        private int CalcularPrioridad(ClienteProductoDTO producto)
        {
            // Prioridades según estado y fecha
            int prioridadBase = producto.Estado switch
            {
                "Pendiente" => 100,
                "En Revision" => 90,
                "Aprobado" => 80,
                "Rechazado" => 10,
                "Compensado" => 5,
                _ => 0
            };

            // Aumentar prioridad por antigüedad (más reciente = mayor prioridad)
            if (producto.FechaReclamoClienteFinal.HasValue)
            {
                var diasDesdeReclamo = (DateTime.Now - producto.FechaReclamoClienteFinal.Value).Days;
                prioridadBase += Math.Max(0, 30 - diasDesdeReclamo); // Hasta 30 puntos por antigüedad
            }

            return prioridadBase;
        }

        private List<ClienteReclamoDTO> OrdenarReclamosPorRelevancia(List<ClienteReclamoDTO> reclamos)
        {
            return reclamos.OrderByDescending(r =>
                r.Productos.Any(p => p.Prioridad >= 50) ? 1 : 0) // Primero reclamos con productos relevantes
                .ThenByDescending(r => r.FechaCreacion)
                .ThenByDescending(r => r.Productos.Max(p => p.Prioridad))
                .ToList();
        }

        private void ActualizarEstadisticas(EstadisticasClienteDTO estadisticas, ClienteProductoDTO producto)
        {
            switch (producto.Estado)
            {
                case "Pendiente": estadisticas.ProductosPendientes++; break;
                case "En Revision": estadisticas.ProductosEnRevision++; break;
                case "Aprobado": estadisticas.ProductosAprobados++; break;
                case "Rechazado": estadisticas.ProductosRechazados++; break;
                case "Compensado": estadisticas.ProductosCompensados++; break;
            }

            if (producto.Compensacion != null)
            {
                if (producto.Compensacion.Tipo == "Reembolso")
                    estadisticas.ReembolsosTotales++;
                else if (producto.Compensacion.Tipo == "Reemplazo")
                    estadisticas.ReemplazosTotales++;
            }
        }

        public async Task<string?> ObtenerPdfBase64Async(string rutaPdf)
        {
            try
            {
                if (string.IsNullOrEmpty(rutaPdf) || !File.Exists(rutaPdf))
                    return null;

                var bytes = await File.ReadAllBytesAsync(rutaPdf);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al leer PDF: {rutaPdf}");
                return null;
            }
        }
    }
}