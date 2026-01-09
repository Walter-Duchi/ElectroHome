using Application.DTOs.Tecnico;

namespace Infrastructure.Services
{
    public interface ITecnicoService
    {
        Task<List<TecnicoProductosResponse>> ObtenerProductosAsignadosAsync(int tecnicoId);
        Task<TecnicoProductosResponse?> ObtenerProximoProductoAsync(int tecnicoId);
        Task<bool> IniciarRevisionAsync(IniciarRevisionRequest request);
        Task<bool> FinalizarRevisionAsync(FinalizarRevisionRequest request);
        Task<bool> ValidarOrdenRevisacionAsync(int reclamoProductoSnId, int tecnicoId);
    }
}