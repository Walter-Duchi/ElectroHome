export interface Ubicacion {
    id: number;
    codigo: string;
    nombre: string;
    tipo: string | null;
    ubicacionPadreId: number | null;
    ubicacionPadreNombre: string | null;
    capacidadMaxima: number | null;
    activo: boolean;
}

export interface CreateUbicacionRequest {
    codigo: string;
    nombre: string;
    tipo?: string;
    ubicacionPadreId?: number;
    capacidadMaxima?: number;
    activo?: boolean;
}

export interface UpdateUbicacionRequest extends CreateUbicacionRequest {
    id: number;
}

export interface MovimientoInventario {
    id: number;
    productoId: number;
    productoNombre: string;
    productoSku: string;
    usuarioId: number | null;
    usuarioNombre: string;
    tipoMovimiento: string;
    cantidad: number;
    cantidadAnterior: number;
    cantidadNueva: number;
    motivo: string | null;
    referencia: string | null;
    fechaMovimiento: string;
    costoUnitario: number | null;
}

export interface CreateMovimientoRequest {
    productoId: number;
    tipoMovimiento: string;
    cantidad: number;
    motivo?: string;
    referencia?: string;
    costoUnitario?: number;
    numerosSerie?: CreateNumeroSerieRequest[];
}

export interface CreateNumeroSerieRequest {
    numeroSerie: string;
    proveedorId: number;
    ubicacionId?: number;
}

export interface NumeroSerie {
    id: number;
    productoId: number;
    productoNombre: string;
    numeroSerie: string;
    estadoInventario: string;
    fechaIngreso: string;
    proveedorId: number;
    proveedorNombre: string;
    ubicacionId: number | null;
    ubicacionNombre: string | null;
}

export interface UpdateNumeroSerieRequest {
    id: number;
    ubicacionId?: number;
    estadoInventario: string;
}

export interface Proveedor {
    id: number;
    nombre: string;
    cedula: string;
    ruc: string;
    direccion: string | null;
    telefono: string | null;
    email: string | null;
    contactoPrincipal: string | null;
    plazoEntregaDias: number | null;
    activo: boolean;
    fechaCreacion: string;
}

export interface CreateProveedorRequest {
    nombre: string;
    cedula: string;
    ruc: string;
    direccion?: string;
    telefono?: string;
    email?: string;
    contactoPrincipal?: string;
    plazoEntregaDias?: number;
    activo?: boolean;
}

export interface UpdateProveedorRequest extends CreateProveedorRequest {
    id: number;
}
