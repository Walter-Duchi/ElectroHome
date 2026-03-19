import api from './api';
import type { DatosEmpresa, UpdateDatosEmpresaRequest } from '../src/types/empresa';

export const empresaService = {
    async obtenerDatosEmpresa(): Promise<DatosEmpresa> {
        try {
            const response = await api.get<DatosEmpresa>('/admin/datos-empresa');
            return response.data;
        } catch (error: any) {
            if (error.response?.status === 404) {
                // No hay datos configurados, retornar un objeto vacío
                return {
                    id: 0,
                    rucEmpresa: '',
                    nombreComercial: '',
                    razonSocial: '',
                    direccionMatriz: ''
                };
            }
            throw error;
        }
    },

    async actualizarDatosEmpresa(data: UpdateDatosEmpresaRequest): Promise<DatosEmpresa> {
        const response = await api.put<DatosEmpresa>('/admin/datos-empresa', data);
        return response.data;
    }
};
