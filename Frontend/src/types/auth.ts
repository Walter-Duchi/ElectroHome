export interface LoginRequest {
    correo: string;
    contrasena: string;
}

export interface LoginResponse {
    token: string;
    id: number;
    correo: string;
    rol: string;
    nombres: string;
    apellidos: string;
}

export interface User {
    id: number;
    correo: string;
    rol: string;
    nombres: string;
    apellidos: string;
}

export interface AuthState {
    user: User | null;
    token: string | null;
    isAuthenticated: boolean;
}

export interface ForgotPasswordRequest {
    correo: string;
}

export interface ResetPasswordRequest {
    token: string;
    nuevaContrasena: string;
    confirmarContrasena: string;
}

export interface ValidateTokenResponse {
    valid: boolean;
    message: string;
}
