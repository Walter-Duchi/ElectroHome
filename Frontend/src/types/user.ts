export interface CreateUserRequest {
    nombres: string;
    apellidos: string;
    razonSocial?: string;
    tipoIdentificacion: 'Cedula' | 'Pasaporte';
    identificacion: string;
    ruc?: string;
    correo: string;
    celular: string;
    convencional?: string;
    ciudad: string;
    codigoPostal: string;
    direccion: string;
    rol: string;
    numCuentaBancaria: string;
    tipoCuentaBancaria: 'Ahorro' | 'Corriente';
    contribuyenteEspecial: boolean;
    obligadoContabilidad: boolean;
}

export interface CreateUserResponse {
    id: number;
    nombres: string;
    apellidos: string;
    correo: string;
    celular: string;
    rol: string;
    fechaCreacion: string;
    contrasenaGenerada: string;
    mensaje: string;
}

export interface AllowedRolesResponse {
    allowedRoles: string[];
}
