import api from './api';
import type {
    ValidarClienteRequest,
    ValidarClienteResponse,
    ValidarProductoRequest,
    ValidarProductoResponse,
    CrearReclamoRequest,
    CrearReclamoResponse,
    ProductoCompradoDTO
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

    async obtenerProductosComprados(identificador: string): Promise<ProductoCompradoDTO[]> {
        try {
            const response = await api.post<ProductoCompradoDTO[]>('/reclamos/productos-comprados', { identificador });
            return response.data;
        } catch (error: any) {
            console.error('Error obteniendo productos comprados:', error);
            return [];
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
            console.log('========================================');
            console.log('ENVIANDO SOLICITUD PARA CREAR RECLAMO');
            console.log('Identificador Cliente:', request.identificadorCliente);
            console.log('Productos a enviar:', request.productos);
            console.log('Solicitud completa:', JSON.stringify(request, null, 2));
            console.log('========================================');

            const response = await api.post<CrearReclamoResponse>('/reclamos/crear', request);

            console.log('========================================');
            console.log('RESPUESTA DEL SERVIDOR:', response.data);
            console.log('========================================');

            return response.data;
        } catch (error: any) {
            console.error('========================================');
            console.error('ERROR CREANDO RECLAMO');
            console.error('Código de error:', error.code);
            console.error('Mensaje:', error.message);
            console.error('URL:', error.config?.url);
            console.error('Método:', error.config?.method);
            console.error('Datos enviados:', error.config?.data);

            if (error.response) {
                console.error('Respuesta del servidor:', error.response.data);
                console.error('Status:', error.response.status);
                console.error('Headers:', error.response.headers);
            }

            if (error.request) {
                console.error('Request:', error.request);
            }

            console.error('========================================');

            return {
                exito: false,
                mensaje: error.response?.data?.mensaje || `Error al crear el reclamo: ${error.message}`
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

            // Intentar guardar en carpeta Documentos/reclamos
            try {
                // Para Chrome/Edge (experimental)
                if ('showSaveFilePicker' in window) {
                    // @ts-ignore
                    window.showSaveFilePicker({
                        suggestedName: fileName,
                        types: [{
                            description: 'PDF Files',
                            accept: { 'application/pdf': ['.pdf'] }
                        }]
                    }).then(async (handle: any) => {
                        const writable = await handle.createWritable();
                        await writable.write(blob);
                        await writable.close();
                    }).catch(() => {
                        // Fallback si el usuario cancela
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                    });
                } else {
                    // Fallback para otros navegadores
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                }
            } catch (error) {
                // Fallback si hay error
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }

            window.URL.revokeObjectURL(url);

            // Mostrar mensaje informativo
            alert('PDF descargado. Se recomienda guardarlo en la carpeta Documentos/reclamos para mejor organización.');
        } catch (error) {
            console.error('Error descargando PDF:', error);
            alert('Error al descargar el PDF. Por favor, contacte al administrador.');
        }
    }
};