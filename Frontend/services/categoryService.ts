import api from './api';
import { Category } from '../src/types/ecommerce';

export const categoryService = {
    async getAllCategories(): Promise<Category[]> {
        const response = await api.get('/ecommerce/categorias');
        return response.data;
    },

    async getCategoryById(id: number): Promise<Category> {
        const response = await api.get(`/ecommerce/categorias/${id}`);
        return response.data;
    }
};
