import api from './api';
import type { DashboardAnalista } from '../src/types/analista';

export const analistaService = {
    async getDashboard(): Promise<DashboardAnalista> {
        const response = await api.get('/analista/dashboard');
        return response.data;
    },

    async exportarVentas(desde?: string, hasta?: string): Promise<void> {
        const params = new URLSearchParams();
        if (desde) params.append('desde', desde);
        if (hasta) params.append('hasta', hasta);
        const response = await api.get('/analista/exportar/ventas', {
            params,
            responseType: 'blob'
        });
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `ventas_${new Date().toISOString().slice(0, 10)}.csv`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    },

    async exportarInventario(): Promise<void> {
        const response = await api.get('/analista/exportar/inventario', {
            responseType: 'blob'
        });
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `inventario_${new Date().toISOString().slice(0, 10)}.csv`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    }
};
