import api from './api';
import type { CartItem, AddToCartRequest } from '../src/types/ecommerce';

export const cartService = {
    async getCart(): Promise<CartItem[]> {
        const response = await api.get('/ecommerce/carrito');
        return response.data;
    },

    async addToCart(request: AddToCartRequest): Promise<void> {
        await api.post('/ecommerce/carrito', request);
    },

    async updateQuantity(productoId: number, cantidad: number): Promise<void> {
        await api.put(`/ecommerce/carrito/${productoId}?cantidad=${cantidad}`);
    },

    async removeItem(productoId: number): Promise<void> {
        await api.delete(`/ecommerce/carrito/${productoId}`);
    },

    async clearCart(): Promise<void> {
        await api.delete('/ecommerce/carrito');
    }
};
