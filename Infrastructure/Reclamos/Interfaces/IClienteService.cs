using Application.DTOs.Reclamos.Cliente;
using System.Security.Claims;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteDashboardResponse> ObtenerDashboardClienteAsync(int clienteId, ClienteDashboardRequest? filtros = null);
        Task<string?> ObtenerPdfBase64Async(string rutaPdf);
    }
}