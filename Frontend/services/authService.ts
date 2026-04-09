import api from './api';
import { type LoginRequest, type LoginResponse } from '../src/types/auth';

export const authService = {
    async login(credentials: LoginRequest): Promise<LoginResponse> {
        try {
            const response = await api.post<LoginResponse>('/auth/login', credentials);
            if (response.data.token) {
                localStorage.setItem('token', response.data.token);
                localStorage.setItem('user', JSON.stringify({
                    id: response.data.id,
                    correo: response.data.correo,
                    rol: response.data.rol,
                    nombres: response.data.nombres,
                    apellidos: response.data.apellidos
                }));
            }
            return response.data;
        } catch (error: any) {
            if (error.response?.status === 401) {
                const errorMessage = error.response?.data?.message || error.response?.data;
                if (errorMessage.includes('desactivada') || errorMessage.includes('inactiva')) {
                    throw new Error('Cuenta desactivada. Contacte al administrador.');
                }
                throw new Error('Credenciales incorrectas');
            }
            if (error.response?.status === 400) {
                throw new Error('Datos de inicio de sesión inválidos');
            }
            throw new Error('Error en el servidor. Intente nuevamente.');
        }
    },

    async register(userData: any): Promise<void> {
        await api.post('/auth/register', userData);
    },

    logout(): void {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/';
    },

    getCurrentUser() {
        const userStr = localStorage.getItem('user');
        return userStr ? JSON.parse(userStr) : null;
    },

    isAuthenticated(): boolean {
        return !!localStorage.getItem('token');
    },

    getAuthHeader() {
        const token = localStorage.getItem('token');
        return token ? { Authorization: `Bearer ${token}` } : {};
    }
};