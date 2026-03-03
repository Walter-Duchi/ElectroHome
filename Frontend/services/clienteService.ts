import api from './api';
import type {
    ClienteDashboardResponse,
    ClienteDashboardRequest,
    PdfResponse
} from '../src/types/cliente';

export const clienteService = {
    async obtenerDashboard(filtros?: ClienteDashboardRequest): Promise<ClienteDashboardResponse> {
        try {
            const response = await api.post('/cliente/dashboard', filtros || {});
            return response.data;
        } catch (error: any) {
            console.error('Error al obtener dashboard del cliente:', error);
            throw new Error(error.response?.data?.message || 'Error al cargar el dashboard');
        }
    },

    async obtenerPdf(tipo: 'tecnico' | 'entrega', nombreArchivo: string): Promise<PdfResponse> {
        try {
            const response = await api.get(`/cliente/pdf/${tipo}/${nombreArchivo}`);
            return response.data;
        } catch (error: any) {
            console.error('Error al obtener PDF:', error);
            throw new Error(error.response?.data?.message || 'Error al cargar el PDF');
        }
    },

    descargarPdf(base64: string, nombreArchivo: string): void {
        try {
            const link = document.createElement('a');
            link.href = `data:application/pdf;base64,${base64}`;
            link.download = nombreArchivo;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        } catch (error) {
            console.error('Error al descargar PDF:', error);
        }
    },

    verPdfEnNuevaVentana(base64: string, nombreArchivo: string): void {
        try {
            const pdfWindow = window.open();
            if (pdfWindow) {
                pdfWindow.document.write(`
                    <html>
                        <head>
                            <title>${nombreArchivo}</title>
                            <style>
                                body { margin: 0; padding: 0; }
                                embed { width: 100%; height: 100vh; border: none; }
                            </style>
                        </head>
                        <body>
                            <embed src="data:application/pdf;base64,${base64}#toolbar=1&navpanes=1&scrollbar=1">
                        </body>
                    </html>
                `);
                pdfWindow.document.close();
            }
        } catch (error) {
            console.error('Error al mostrar PDF:', error);
        }
    },

    getEstadoColor(estado: string): string {
        switch (estado) {
            case 'Pendiente': return 'warning';
            case 'En Revision': return 'info';
            case 'Aprobado': return 'success';
            case 'Rechazado': return 'error';
            case 'Compensado': return 'secondary';
            default: return 'default';
        }
    },

    getTipoReclamoColor(tipo: string): string {
        switch (tipo) {
            case 'Reembolso': return 'primary';
            case 'Reemplazo': return 'secondary';
            default: return 'default';
        }
    },

    formatearFecha(fecha: string | null): string {
        if (!fecha) return '-';
        return new Date(fecha).toLocaleDateString('es-ES', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
};
