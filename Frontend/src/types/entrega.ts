export interface ProductoEntregaDTO {
    reclamoProductoSnId: number;
    numeroSerieProductoDefectuoso: string;
    marca: string;
    modelo: string;
    estado: string;
    formaCompensacion: string;
    numeroSerieReemplazo?: string;
    productoReemplazoId?: number;
    reemplazoValido: boolean;
    mensajeValidacion?: string;
}

export interface BuscarReclamoResponse {
    exito: boolean;
    mensaje: string;
    codigoReclamo: string;
    cliente: string;
    ruc: string;
    productos: ProductoEntregaDTO[];
    todosProductosRevisados: boolean;
    totalProductosReclamo: number;
    productosPendientesRevision: number;
}

export interface ProductoReemplazoDTO {
    id: number;
    numeroSerie: string;
    marca: string;
    modelo: string;
    estadoInventario: string;
}

export interface ValidarReemplazoResponse {
    valido: boolean;
    mensaje: string;
    productoReemplazo?: ProductoReemplazoDTO;
}

export interface ProductoEntregaComprobanteDTO {
    numeroSerieDefectuoso: string;
    marca: string;
    modelo: string;
    numeroSerieReemplazo: string;
}

export interface ComprobanteEntregaDTO {
    codigoReclamo: string;
    cliente: string;
    ruc: string;
    fechaEntrega: string;
    personalEntrega: string;
    productos: ProductoEntregaComprobanteDTO[];
    firmaBase64: string;
}

export interface ReclamoPendienteEntregaDTO {
    id: number;
    codigoReclamo: string;
    cliente: string;
    ruc: string;
    fechaCreacion: string;
    cantidadProductosPendientes: number;
}