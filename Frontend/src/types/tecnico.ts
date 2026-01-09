export interface TecnicoProducto {
    id: number;
    numeroSerie: string;
    marca: string;
    modelo: string;
    especificacion: string;
    estado: 'Pendiente' | 'En Revision' | 'Aprobado' | 'Rechazado';
    fechaReclamoClienteFinal: string;
    codigoReclamo: string;
    formaCompensacion: 'Reembolso' | 'Reemplazo';
    precio: number;
    clienteNombre: string;
    clienteRuc: string;
    fechaVentaClienteFinal: string;
    diasGarantia: number;
    garantiaValida: boolean;
}

export interface IniciarRevisionRequest {
    reclamoProductoSnId: number;
    tecnicoId: number;
}

export interface FinalizarRevisionRequest {
    reclamoProductoSnId: number;
    tecnicoId: number;
    estado: 'Aprobado' | 'Rechazado';
    explicacion: string;
    pdfBase64?: string;
    pdfFileName?: string;
}

export interface ValidarOrdenResponse {
    valido: boolean;
    message: string;
}
