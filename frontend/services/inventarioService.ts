import api from './api';
import type {
    Ubicacion, CreateUbicacionRequest, UpdateUbicacionRequest,
    MovimientoInventario, CreateMovimientoRequest,
    NumeroSerie, UpdateNumeroSerieRequest,
    Proveedor, CreateProveedorRequest, UpdateProveedorRequest
} from '../src/types/inventario';

export const inventarioService = {
    // Ubicaciones
    async getUbicaciones(): Promise<Ubicacion[]> {
        const response = await api.get('/inventario/ubicaciones');
        return response.data;
    },

    async getUbicacion(id: number): Promise<Ubicacion> {
        const response = await api.get(`/inventario/ubicaciones/${id}`);
        return response.data;
    },

    async createUbicacion(data: CreateUbicacionRequest): Promise<Ubicacion> {
        const response = await api.post('/inventario/ubicaciones', data);
        return response.data;
    },

    async updateUbicacion(data: UpdateUbicacionRequest): Promise<Ubicacion> {
        const response = await api.put('/inventario/ubicaciones', data);
        return response.data;
    },

    async deleteUbicacion(id: number): Promise<void> {
        await api.delete(`/inventario/ubicaciones/${id}`);
    },

    // Movimientos
    async getMovimientos(productoId?: number, desde?: string, hasta?: string): Promise<MovimientoInventario[]> {
        const params = new URLSearchParams();
        if (productoId) params.append('productoId', productoId.toString());
        if (desde) params.append('desde', desde);
        if (hasta) params.append('hasta', hasta);
        const response = await api.get('/inventario/movimientos', { params });
        return response.data;
    },

    async registrarEntrada(data: CreateMovimientoRequest): Promise<MovimientoInventario> {
        const response = await api.post('/inventario/entrada', data);
        return response.data;
    },

    async registrarSalida(data: CreateMovimientoRequest): Promise<MovimientoInventario> {
        const response = await api.post('/inventario/salida', data);
        return response.data;
    },

    async registrarAjuste(data: CreateMovimientoRequest): Promise<MovimientoInventario> {
        const response = await api.post('/inventario/ajuste', data);
        return response.data;
    },

    async registrarDevolucion(data: CreateMovimientoRequest): Promise<MovimientoInventario> {
        const response = await api.post('/inventario/devolucion', data);
        return response.data;
    },

    // Números de serie
    async getNumerosSerie(productoId?: number, estado?: string, ubicacionId?: number): Promise<NumeroSerie[]> {
        const params = new URLSearchParams();
        if (productoId) params.append('productoId', productoId.toString());
        if (estado) params.append('estado', estado);
        if (ubicacionId) params.append('ubicacionId', ubicacionId.toString());
        const response = await api.get('/inventario/numeros-serie', { params });
        return response.data;
    },

    async getNumeroSerie(numero: string): Promise<NumeroSerie> {
        const response = await api.get(`/inventario/numeros-serie/${numero}`);
        return response.data;
    },

    async updateNumeroSerie(data: UpdateNumeroSerieRequest): Promise<void> {
        await api.put('/inventario/numeros-serie', data);
    },

    // Proveedores
    async getProveedores(soloActivos: boolean = true): Promise<Proveedor[]> {
        const response = await api.get('/inventario/proveedores', { params: { soloActivos } });
        return response.data;
    },

    async getProveedor(id: number): Promise<Proveedor> {
        const response = await api.get(`/inventario/proveedores/${id}`);
        return response.data;
    },

    async createProveedor(data: CreateProveedorRequest): Promise<Proveedor> {
        const response = await api.post('/inventario/proveedores', data);
        return response.data;
    },

    async updateProveedor(data: UpdateProveedorRequest): Promise<Proveedor> {
        const response = await api.put('/inventario/proveedores', data);
        return response.data;
    },

    async toggleProveedorActivo(id: number, activo: boolean): Promise<void> {
        await api.patch(`/inventario/proveedores/${id}/toggle?activo=${activo}`);
    },

    async deleteProveedor(id: number): Promise<void> {
        await api.delete(`/inventario/proveedores/${id}`);
    }
};
