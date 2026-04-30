import api from './api';
import {
    type BuscarReclamoResponse,
    type ValidarReemplazoResponse,
    type ComprobanteEntregaDTO,
    type ReclamoPendienteEntregaDTO,
} from '../src/types/entrega';

export const entregaService = {
    // Obtener reclamos pendientes de entrega
    obtenerReclamosPendientes: async (): Promise<ReclamoPendienteEntregaDTO[]> => {
        const response = await api.get('/entrega/reclamos-pendientes');
        return response.data;
    },

    // Buscar reclamo por código
    buscarReclamo: async (codigoReclamo: string): Promise<BuscarReclamoResponse> => {
        const response = await api.post('/entrega/buscar-reclamo', { codigoReclamo });
        return response.data;
    },

    // Asignar reemplazos automáticamente
    asignarReemplazosAutomatico: async (codigoReclamo: string): Promise<{ exito: boolean }> => {
        const response = await api.post('/entrega/asignar-reemplazos-automatico', { codigoReclamo });
        return response.data;
    },

    // Validar producto de reemplazo
    validarReemplazo: async (reclamoProductoSnId: number, numeroSerieReemplazo: string): Promise<ValidarReemplazoResponse> => {
        const response = await api.post('/entrega/validar-reemplazo', {
            reclamoProductoSnId,
            numeroSerieReemplazo
        });
        return response.data;
    },

    // Seleccionar producto de reemplazo
    seleccionarReemplazo: async (reclamoProductoSnId: number, numeroSerieReemplazo: string): Promise<void> => {
        await api.post('/entrega/seleccionar-reemplazo', {
            reclamoProductoSnId,
            numeroSerieReemplazo
        });
    },

    // Verificar si todos los productos tienen reemplazo
    verificarReemplazos: async (codigoReclamo: string): Promise<{ todosTienenReemplazo: boolean }> => {
        const response = await api.get(`/entrega/verificar-reemplazos/${codigoReclamo}`);
        return response.data;
    },

    // Generar datos para comprobante
    generarDatosComprobante: async (codigoReclamo: string): Promise<ComprobanteEntregaDTO> => {
        const response = await api.post('/entrega/generar-datos-comprobante', { codigoReclamo });
        return response.data;
    },

    // Generar PDF de comprobante
    generarPdfComprobante: async (comprobante: ComprobanteEntregaDTO): Promise<{ rutaPdf: string }> => {
        const response = await api.post('/entrega/generar-pdf-comprobante', comprobante);
        return response.data;
    },

    // Subir comprobante firmado
    subirComprobante: async (codigoReclamo: string, pdfBase64: string): Promise<void> => {
        await api.post('/entrega/subir-comprobante', {
            codigoReclamo,
            pdfBase64
        });
    },

    // Confirmar entrega
    confirmarEntrega: async (codigoReclamo: string): Promise<void> => {
        await api.post('/entrega/confirmar-entrega', { codigoReclamo });
    },
};