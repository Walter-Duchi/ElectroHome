import api from './api';
import {
    ValidarClienteRequest,
    ValidarClienteResponse,
    ValidarProductoRequest,
    ValidarProductoResponse,
    CrearReclamoRequest,
    CrearReclamoResponse
} from '../src/types/reclamo';

export const reclamoService = {
    async validarCliente(request: ValidarClienteRequest): Promise<ValidarClienteResponse> {
        try {
            const response = await api.post<ValidarClienteResponse>('/reclamos/validar-cliente', request);
            return response.data;
        } catch (error: any) {
            console.error('Error validando cliente:', error);
            return {
                esValido: false,
                mensaje: error.response?.data?.mensaje || 'Error al validar cliente'
            };
        }
    },

    async validarProducto(request: ValidarProductoRequest): Promise<ValidarProductoResponse> {
        try {
            const response = await api.post<ValidarProductoResponse>('/reclamos/validar-producto', request);
            return response.data;
        } catch (error: any) {
            console.error('Error validando producto:', error);
            return {
                esValido: false,
                mensaje: error.response?.data?.mensaje || 'Error al validar producto',
                tieneGarantia: false
            };
        }
    },

    async crearReclamo(request: CrearReclamoRequest): Promise<CrearReclamoResponse> {
        try {
            const response = await api.post<CrearReclamoResponse>('/reclamos/crear', request);
            return response.data;
        } catch (error: any) {
            console.error('Error creando reclamo:', error);
            return {
                exito: false,
                mensaje: error.response?.data?.mensaje || 'Error al crear el reclamo'
            };
        }
    },

    descargarPdf(base64: string, fileName: string): void {
        try {
            const byteCharacters = atob(base64);
            const byteNumbers = new Array(byteCharacters.length);
            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            const byteArray = new Uint8Array(byteNumbers);
            const blob = new Blob([byteArray], { type: 'application/pdf' });
            const url = window.URL.createObjectURL(blob);

            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error('Error descargando PDF:', error);
        }
    }
};
