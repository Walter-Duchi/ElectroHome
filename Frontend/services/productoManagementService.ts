import api from './api';
import type {
    ProductoManagement, CreateProductoRequest, UpdateProductoRequest,
    Categoria, CreateCategoriaRequest, UpdateCategoriaRequest,
    Marca, CreateMarcaRequest, UpdateMarcaRequest
} from '../src/types/producto';

export const productoManagementService = {
    // Productos
    async getProductos(includeInactivos: boolean = false): Promise<ProductoManagement[]> {
        const response = await api.get('/productos/gestion', { params: { includeInactivos } });
        return response.data;
    },

    async getProducto(id: number): Promise<ProductoManagement> {
        const response = await api.get(`/productos/gestion/${id}`);
        return response.data;
    },

    async createProducto(data: CreateProductoRequest): Promise<ProductoManagement> {
        const formData = new FormData();
        Object.entries(data).forEach(([key, value]) => {
            if (value !== undefined && value !== null) {
                if (key === 'imagenPrincipal' && value instanceof File) {
                    formData.append('ImagenPrincipal', value);
                } else if (key === 'imagenesAdicionales' && Array.isArray(value)) {
                    value.forEach(file => formData.append('ImagenesAdicionales', file));
                } else {
                    formData.append(key, value.toString());
                }
            }
        });
        const response = await api.post('/productos/gestion', formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        return response.data;
    },

    async updateProducto(data: UpdateProductoRequest): Promise<ProductoManagement> {
        const formData = new FormData();
        Object.entries(data).forEach(([key, value]) => {
            if (value !== undefined && value !== null) {
                if (key === 'imagenPrincipal' && value instanceof File) {
                    formData.append('ImagenPrincipal', value);
                } else if (key === 'imagenesAdicionales' && Array.isArray(value)) {
                    value.forEach(file => formData.append('ImagenesAdicionales', file));
                } else if (key === 'imagenesAEliminar' && Array.isArray(value)) {
                    formData.append('ImagenesAEliminar', value.join(','));
                } else {
                    formData.append(key, value.toString());
                }
            }
        });
        const response = await api.put('/productos/gestion', formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        return response.data;
    },

    async toggleProductoActivo(id: number, activo: boolean): Promise<void> {
        await api.patch(`/productos/gestion/${id}/toggle?activo=${activo}`);
    },

    // Categorías
    async getCategorias(includeInactivos: boolean = false): Promise<Categoria[]> {
        const response = await api.get('/productos/categorias', { params: { includeInactivos } });
        return response.data;
    },

    async getCategoria(id: number): Promise<Categoria> {
        const response = await api.get(`/productos/categorias/${id}`);
        return response.data;
    },

    async createCategoria(data: CreateCategoriaRequest): Promise<Categoria> {
        const response = await api.post('/productos/categorias', data);
        return response.data;
    },

    async updateCategoria(data: UpdateCategoriaRequest): Promise<Categoria> {
        const response = await api.put('/productos/categorias', data);
        return response.data;
    },

    async deleteCategoria(id: number): Promise<void> {
        await api.delete(`/productos/categorias/${id}`);
    },

    // Marcas
    async getMarcas(): Promise<Marca[]> {
        const response = await api.get('/productos/marcas');
        return response.data;
    },

    async getMarca(id: number): Promise<Marca> {
        const response = await api.get(`/productos/marcas/${id}`);
        return response.data;
    },

    async createMarca(data: CreateMarcaRequest): Promise<Marca> {
        const response = await api.post('/productos/marcas', data);
        return response.data;
    },

    async updateMarca(data: UpdateMarcaRequest): Promise<Marca> {
        const response = await api.put('/productos/marcas', data);
        return response.data;
    },

    async deleteMarca(id: number): Promise<void> {
        await api.delete(`/productos/marcas/${id}`);
    }
};
