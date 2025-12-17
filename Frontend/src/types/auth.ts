// src/types/auth.ts
export interface LoginRequest {
    correo: string;
    contrasena: string;
}

export interface LoginResponse {
    token: string;
    id: number;
    correo: string;
    rol: string;
}

export interface User {
    id: number;
    correo: string;
    rol: string;
}

export interface AuthState {
    user: User | null;
    token: string | null;
    isAuthenticated: boolean;
}