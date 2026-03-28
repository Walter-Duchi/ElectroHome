import api from './api';
import { type CreateUserRequest, type CreateUserResponse, type ProfileResponse, type UpdateProfileRequest } from '../src/types/user';
export const userService = {
    async createUser(userData: CreateUserRequest): Promise<CreateUserResponse> {
        try {
            const response = await api.post<CreateUserResponse>('/admin/crear-usuario', userData);
            return response.data;
        } catch (error: any) {
            if (error.response?.data?.message) {
                throw new Error(error.response.data.message);
            }
            throw new Error('Error al crear usuario');
        }
    },

    async getAdminAllowedRoles(): Promise<string[]> {
        try {
            const response = await api.get<string[]>('/admin/roles-permitidos');
            return response.data;
        } catch (error) {
            console.error('Error al obtener roles permitidos para administrador:', error);
            return [];
        }
    },

    validateCedula(cedula: string): boolean {
        if (!cedula || cedula.length !== 10 || !/^\d+$/.test(cedula)) {
            return false;
        }

        // Algoritmo módulo 10 para cédula ecuatoriana
        const coeficientes = [2, 1, 2, 1, 2, 1, 2, 1, 2];
        let total = 0;

        for (let i = 0; i < 9; i++) {
            let valor = parseInt(cedula[i]) * coeficientes[i];
            if (valor >= 10) valor -= 9;
            total += valor;
        }

        let digitoVerificador = total % 10;
        if (digitoVerificador !== 0) {
            digitoVerificador = 10 - digitoVerificador;
        }

        return digitoVerificador === parseInt(cedula[9]);
    },

    validateBankAccount(accountNumber: string): boolean {
        if (!accountNumber) return false;
        const cleaned = accountNumber.replace(/\D/g, '');
        return cleaned.length >= 10 && cleaned.length <= 20 && /^\d+$/.test(cleaned);
    },

    validateAccountType(accountType: string): boolean {
        return accountType === 'Ahorro' || accountType === 'Corriente';
    },

    validatePostalCode(codigoPostal: string): boolean {
        // Validar que sea un código postal válido para Guayaquil (6 dígitos)
        return /^\d{6}$/.test(codigoPostal);
    },

    async getProfile(): Promise<ProfileResponse> {
        try {
            const response = await api.get<ProfileResponse>('/user/profile');
            return response.data;
        } catch (error: any) {
            if (error.response?.data?.message) {
                throw new Error(error.response.data.message);
            }
            throw new Error('Error al obtener perfil');
        }
    },

    async updateProfile(data: UpdateProfileRequest): Promise<void> {
        try {
            await api.put('/user/profile', data);
        } catch (error: any) {
            if (error.response?.data?.message) {
                throw new Error(error.response.data.message);
            }
            throw new Error('Error al actualizar perfil');
        }
    }
};
