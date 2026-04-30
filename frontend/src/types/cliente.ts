export interface ClienteDashboardResponse {
    reclamos: ClienteReclamoDTO[];
    estadisticas: EstadisticasClienteDTO;
}

export interface ClienteReclamoDTO {
    reclamoId: number;
    codigoReclamo: string;
    fechaCreacion: string;
    productos: ClienteProductoDTO[];
}

export interface ClienteProductoDTO {
    reclamoProductoId: number;
    marca: string;
    modelo: string;
    numeroSerie: string;
    tipoReclamo: string;
    estado: string;
    fechaVentaClienteFinal: string | null;
    fechaReclamoClienteFinal: string | null;
    tecnicoId: number | null;
    tecnicoNombre: string | null;
    fechaRevisionTecnico: string | null;
    explicacionRespuestaTecnico: string | null;
    pdfRevisionTecnico: string | null;
    compensacion: CompensacionDTO | null;
    prioridad: number;
}

export interface CompensacionDTO {
    tipo: string;
    numeroComprobanteReembolso: string | null;
    fechaReembolso: string | null;
    numCuentaBancariaReembolso: string | null;
    numeroSerieReemplazo: string | null;
    pdfComprobanteEntrega: string | null;
    personalEntregaNombre: string | null;
}

export interface EstadisticasClienteDTO {
    totalReclamos: number;
    productosPendientes: number;
    productosEnRevision: number;
    productosAprobados: number;
    productosRechazados: number;
    productosCompensados: number;
    reembolsosTotales: number;
    reemplazosTotales: number;
}

export interface ClienteDashboardRequest {
    codigoReclamo?: string;
    numeroSerie?: string;
    tipoReclamo?: string;
    estado?: string;
    fechaDesde?: string;
    fechaHasta?: string;
    soloPendientes?: boolean;
    soloAprobados?: boolean;
    soloCompensados?: boolean;
    soloReembolsos?: boolean;
    soloReemplazos?: boolean;
}

export interface PdfResponse {
    pdfBase64: string;
    nombreArchivo: string;
}
