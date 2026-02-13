export interface DatosEmpresa {
    id: number;
    rucEmpresa: string;
    nombreComercial: string;
    razonSocial: string;
    direccionMatriz: string;
}

export interface UpdateDatosEmpresaRequest {
    rucEmpresa: string;
    nombreComercial: string;
    razonSocial: string;
    direccionMatriz: string;
}
