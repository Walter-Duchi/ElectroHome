import api from './api';
import {
    TecnicoProducto,
    IniciarRevisionRequest,
    FinalizarRevisionRequest,
    ValidarOrdenResponse
} from '../src/types/tecnico';

export const tecnicoService = {
    // Obtener todos los productos asignados al técnico
    async obtenerProductosAsignados(): Promise<TecnicoProducto[]> {
        try {
            const response = await api.get<TecnicoProducto[]>('/tecnico/productos');
            return response.data;
        } catch (error: any) {
            console.error('Error obteniendo productos del técnico:', error);
            throw new Error(error.response?.data?.message || 'Error al obtener productos');
        }
    },

    // Obtener el próximo producto a revisar
    async obtenerProximoProducto(): Promise<TecnicoProducto | null> {
        try {
            const response = await api.get<TecnicoProducto>('/tecnico/proximo-producto');
            return response.data;
        } catch (error: any) {
            if (error.response?.status === 404) {
                return null;
            }
            console.error('Error obteniendo próximo producto:', error);
            throw new Error(error.response?.data?.message || 'Error al obtener próximo producto');
        }
    },

    // Validar si un producto está en el orden correcto
    async validarOrdenRevisacion(productoId: number): Promise<ValidarOrdenResponse> {
        try {
            const response = await api.get<ValidarOrdenResponse>(`/tecnico/validar-orden/${productoId}`);
            return response.data;
        } catch (error: any) {
            console.error('Error validando orden:', error);
            return {
                valido: false,
                message: error.response?.data?.message || 'Error al validar orden'
            };
        }
    },

    // Iniciar revisión de un producto
    async iniciarRevision(request: IniciarRevisionRequest): Promise<void> {
        try {
            await api.post('/tecnico/iniciar-revision', request);
        } catch (error: any) {
            console.error('Error iniciando revisión:', error);
            throw new Error(error.response?.data?.message || 'Error al iniciar revisión');
        }
    },

    // Finalizar revisión de un producto
    async finalizarRevision(request: FinalizarRevisionRequest): Promise<void> {
        try {
            await api.post('/tecnico/finalizar-revision', request);
        } catch (error: any) {
            console.error('Error finalizando revisión:', error);
            throw new Error(error.response?.data?.message || 'Error al finalizar revisión');
        }
    },

    // Convertir archivo a base64
    async convertirArchivoABase64(file: File): Promise<string> {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsDataURL(file);
            reader.onload = () => {
                const base64String = reader.result as string;
                // Remover el prefijo data:application/pdf;base64,
                const base64 = base64String.split(',')[1];
                resolve(base64);
            };
            reader.onerror = error => reject(error);
        });
    }
};
