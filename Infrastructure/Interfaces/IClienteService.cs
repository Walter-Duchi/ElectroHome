using Application.DTOs.Cliente;
using System.Security.Claims;

namespace Infrastructure.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteDashboardResponse> ObtenerDashboardClienteAsync(int clienteId, ClienteDashboardRequest? filtros = null);
        Task<string?> ObtenerPdfBase64Async(string rutaPdf);
    }
}