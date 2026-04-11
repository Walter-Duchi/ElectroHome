using Application.DTOs.Reclamos.Entrega;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface IEntregaService
    {
        Task<BuscarReclamoResponse> BuscarReclamoAsync(string codigoReclamo);
        Task<ValidarReemplazoResponse> ValidarProductoReemplazoAsync(int reclamoProductoSnId, string numeroSerieReemplazo);
        Task<bool> SeleccionarReemplazoAsync(SeleccionarReemplazoRequest request, int personalEntregaId);
        Task<ComprobanteEntregaDTO> GenerarDatosComprobanteAsync(string codigoReclamo, int personalEntregaId);
        Task<string> GenerarPdfComprobanteAsync(ComprobanteEntregaDTO comprobante);
        Task<bool> SubirComprobanteAsync(SubirComprobanteRequest request, int personalEntregaId);
        Task<bool> ConfirmarEntregaAsync(ConfirmarEntregaRequest request, int personalEntregaId);
        Task<bool> TodosProductosTienenReemplazoAsync(string codigoReclamo);
        Task<List<ReclamoPendienteEntregaDTO>> ObtenerReclamosPendientesEntregaAsync();
        Task<bool> AsignarReemplazosAutomaticamenteAsync(string codigoReclamo, int personalEntregaId);
    }
}