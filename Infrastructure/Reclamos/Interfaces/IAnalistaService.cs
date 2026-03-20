using Application.DTOs.Analista;

namespace Infrastructure.Reclamos.Interfaces;

public interface IAnalistaService
{
    Task<DashboardAnalistaDto> ObtenerDashboardAsync();
    Task<byte[]> ExportarReporteVentasAsync(DateTime? desde, DateTime? hasta);
    Task<byte[]> ExportarReporteInventarioAsync();
}