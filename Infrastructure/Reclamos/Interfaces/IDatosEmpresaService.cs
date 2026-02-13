using Application.DTOs.Admin;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface IDatosEmpresaService
    {
        Task<DatosEmpresaResponse?> ObtenerDatosEmpresaAsync();
        Task<DatosEmpresaResponse> ActualizarDatosEmpresaAsync(UpdateDatosEmpresaRequest request);
    }
}