using Application.DTOs.Reclamos.Cliente;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteDashboardResponse> ObtenerDashboardClienteAsync(int clienteId, ClienteDashboardRequest? filtros = null);
        Task<string?> ObtenerPdfBase64Async(string rutaPdf);
    }
}