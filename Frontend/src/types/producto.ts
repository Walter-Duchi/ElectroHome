export interface ProductoManagement {
    id: number;
    sku: string;
    codigo: string;
    modelo: string;
    nombreCompleto: string;
    marcaId: number;
    marcaNombre: string;
    categoriaId: number | null;
    categoriaNombre: string | null;
    especificacion: string;
    descripcion: string | null;
    precio: number;
    diasGarantia: number;
    visibilidad: string;
    activo: boolean;
    pesoKg: number | null;
    altoCm: number;
    anchoCm: number;
    profundidadCm: number;
    imagenUrl: string | null;
    imagenesAdicionales: string[];
    fechaCreacion: string;
    creadoPor: number | null;
    modificadoPor: number | null;
}

export interface CreateProductoRequest {
    sku: string;
    codigo: string;
    marcaId: number;
    categoriaId?: number;
    modelo: string;
    especificacion: string;
    descripcion?: string;
    precio: number;
    diasGarantia: number;
    visibilidad: string;
    pesoKg?: number;
    altoCm: number;
    anchoCm: number;
    profundidadCm: number;
    imagenPrincipal?: File;
    imagenesAdicionales?: File[];
}

export interface UpdateProductoRequest {
    id: number;
    sku: string;
    codigo: string;
    marcaId: number;
    categoriaId?: number;
    modelo: string;
    especificacion: string;
    descripcion?: string;
    precio: number;
    diasGarantia: number;
    visibilidad: string;
    activo: boolean;
    pesoKg?: number;
    altoCm: number;
    anchoCm: number;
    profundidadCm: number;
    imagenPrincipal?: File;
    imagenesAdicionales?: File[];
    imagenesAEliminar?: string[];
}

export interface Categoria {
    id: number;
    nombre: string;
    descripcion: string | null;
    activo: boolean;
    categoriaPadreId: number | null;
    categoriaPadreNombre: string | null;
    fechaCreacion: string;
}

export interface CreateCategoriaRequest {
    nombre: string;
    descripcion?: string;
    activo?: boolean;
    categoriaPadreId?: number;
}

export interface UpdateCategoriaRequest {
    id: number;
    nombre: string;
    descripcion?: string;
    activo: boolean;
    categoriaPadreId?: number;
}

export interface Marca {
    id: number;
    nombre: string;
}

export interface CreateMarcaRequest {
    nombre: string;
}

export interface UpdateMarcaRequest {
    id: number;
    nombre: string;
}
