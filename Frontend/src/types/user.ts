export interface CreateUserRequest {
    nombres: string;
    apellidos: string;
    correo: string;
    contrasena: string;
    celular: string;
    convencional?: string;
    ruc: string;
    rol: string;
    numCuentaBancaria?: string;
    tipoCuentaBancaria?: string;  // Nuevo campo
}

export interface CreateUserResponse {
    id: number;
    nombres: string;
    apellidos: string;
    correo: string;
    celular: string;
    rol: string;
    fechaCreacion: string;
}

export interface AllowedRolesResponse {
    allowedRoles: string[];
}
