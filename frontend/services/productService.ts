import api from './api';
import type { Product, ProductFilter } from '../src/types/ecommerce';

export const productService = {
    async getProducts(filter: ProductFilter = {}): Promise<Product[]> {
        const response = await api.post('/ecommerce/productos', filter);
        return response.data;
    },

    async getProductById(id: number): Promise<Product> {
        const response = await api.get(`/ecommerce/productos/${id}`);
        return response.data;
    },

    async getPopularProducts(): Promise<Product[]> {
        const response = await api.get('/ecommerce/productos/populares');
        return response.data;
    },

    async getNewArrivals(): Promise<Product[]> {
        const response = await api.get('/ecommerce/productos/nuevos');
        return response.data;
    }
};
