using Application.DTOs.Reclamos.Reclamo;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface IReclamoService
    {
        Task<ValidarClienteResponse> ValidarClienteAsync(string identificador);
        Task<ValidarProductoResponse> ValidarProductoAsync(string numeroSerie);
        Task<List<ProductoCompradoDTO>> ObtenerProductosCompradosAsync(string identificadorCliente);
        Task<CrearReclamoResponse> CrearReclamoAsync(CrearReclamoRequest request, int revisorId);
    }
}