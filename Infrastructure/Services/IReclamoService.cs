using Application.DTOs.Reclamo;

namespace Infrastructure.Services
{
    public interface IReclamoService
    {
        Task<ValidarClienteResponse> ValidarClienteAsync(string ruc);
        Task<ValidarProductoResponse> ValidarProductoAsync(string numeroSerie);
        Task<CrearReclamoResponse> CrearReclamoAsync(CrearReclamoRequest request, int revisorId);
    }
}