export interface ValidarClienteRequest {
    identificador: string;
}

export interface ValidarClienteResponse {
    esValido: boolean;
    mensaje?: string;
    clienteId?: number;
    razonSocial?: string;
}

export interface ProductoCompradoDTO {
    numeroSerie: string;
    marca: string;
    modelo: string;
    precio: number;
    fechaCompra: string;
    diasGarantia: number;
    tieneGarantia: boolean;
}

export interface ValidarProductoRequest {
    numeroSerie: string;
}

export interface ValidarProductoResponse {
    esValido: boolean;
    mensaje?: string;
    productoId?: number;
    marca?: string;
    modelo?: string;
    tieneGarantia: boolean;
    estadoInventario?: string;
    fechaVenta?: string;
    diasGarantia?: number;
    especificacion?: string;
    precio?: number;
}

export interface ProductoReclamadoRequest {
    numeroSerie: string;
    formaCompensacion: 'Reembolso' | 'Reemplazo';
}

export interface CrearReclamoRequest {
    identificadorCliente: string;
    productos: ProductoReclamadoRequest[];
}

export interface CrearReclamoResponse {
    exito: boolean;
    mensaje?: string;
    reclamoId?: number;
    codigoReclamo?: string;
    pdfBase64?: string;
    pdfFileName?: string;
}

export interface ProductoReclamado {
    id: string;
    numeroSerie: string;
    marca?: string;
    modelo?: string;
    tieneGarantia: boolean;
    estadoInventario?: string;
    formaCompensacion: 'Reembolso' | 'Reemplazo';
    especificacion?: string;
    precio?: number;
    validando?: boolean;
    error?: string;
}