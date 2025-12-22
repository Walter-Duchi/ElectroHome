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

    validateCardNumber(cardNumber: string): boolean {
        const cleaned = cardNumber.replace(/\D/g, '');

        if (cleaned.length < 13 || cleaned.length > 19) {
            return false;
        }

        let sum = 0;
        let shouldDouble = false;

        for (let i = cleaned.length - 1; i >= 0; i--) {
            let digit = parseInt(cleaned.charAt(i), 10);

            if (shouldDouble) {
                digit *= 2;
                if (digit > 9) {
                    digit -= 9;
                }
            }

            sum += digit;
            shouldDouble = !shouldDouble;
        }

        return sum % 10 === 0;
    },

    getCardType(cardNumber: string): string {
        const cleaned = cardNumber.replace(/\D/g, '');

        if (/^4/.test(cleaned)) {
            return 'Visa';
        } else if (/^5[1-5]/.test(cleaned)) {
            return 'Mastercard';
        } else if (/^3[47]/.test(cleaned)) {
            return 'American Express';
        } else if (/^3(?:0[0-5]|[68])/.test(cleaned)) {
            return 'Diners Club';
        } else if (/^6(?:011|5)/.test(cleaned)) {
            return 'Discover';
        } else if (/^(?:2131|1800|35)/.test(cleaned)) {
            return 'JCB';
        }

        return 'Desconocida';
    }
};
