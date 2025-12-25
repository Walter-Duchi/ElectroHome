import api from './api';
import { type CreateUserRequest, type CreateUserResponse } from '../src/types/user';

export const userService = {
    async createUser(userData: CreateUserRequest): Promise<CreateUserResponse> {
        try {
            const response = await api.post<CreateUserResponse>('/users/create', userData);
            return response.data;
        } catch (error: any) {
            if (error.response?.data?.message) {
                throw new Error(error.response.data.message);
            }
            throw new Error('Error al crear usuario');
        }
    },

    async getAllowedRoles(): Promise<string[]> {
        try {
            const response = await api.get<string[]>('/users/allowed-roles');
            return response.data;
        } catch (error) {
            console.error('Error al obtener roles permitidos:', error);
            return [];
        }
    },

    validateBankAccount(accountNumber: string): boolean {
        if (!accountNumber) return false;

        // Limpiar el número de cuenta
        const cleaned = accountNumber.replace(/\D/g, '');

        // Validar longitud (10-20 dígitos para cuentas bancarias)
        if (cleaned.length < 10 || cleaned.length > 20) {
            return false;
        }

        // Validar que solo contenga dígitos
        return /^\d+$/.test(cleaned);
    },

    validateAccountType(accountType: string): boolean {
        return accountType === 'Ahorro' || accountType === 'Corriente';
    }
};
