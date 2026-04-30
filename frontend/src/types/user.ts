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

export interface ProfileResponse {
    id: number;
    nombres: string;
    apellidos: string;
    razonSocial?: string;
    tipoIdentificacion: string;
    identificacion: string;
    ruc?: string;
    correo: string;
    celular: string;
    convencional?: string;
    pais: string;
    divisionAdministrativa: string;
    ciudad: string;
    codigoPostal: string;
    direccion: string;
    rol: string;
    fechaCreacion: string;
    numCuentaBancaria: string;
    tipoCuentaBancaria: string;
    contribuyenteEspecial: boolean;
    obligadoContabilidad: boolean;
    activo: boolean;
}

export interface UpdateProfileRequest {
    correo?: string;
    celular?: string;
    convencional?: string;
    ciudad?: string;
    codigoPostal?: string;
    direccion?: string;
    currentPassword?: string;
    newPassword?: string;
    confirmNewPassword?: string;
}