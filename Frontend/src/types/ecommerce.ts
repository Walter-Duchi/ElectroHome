export interface Category {
    id: number;
    nombre: string;
    descripcion?: string;
    categoriaPadreId?: number;
}

export interface Product {
    id: number;
    sku: string;
    nombre: string;
    marca: string;
    categoria?: string;
    descripcion?: string;
    precio: number;
    imagenPrincipal?: string;
    imagenesAdicionales: string[];
    stockDisponible: number;
    diasGarantia: number;
    activo: boolean;
}

export interface CartItem {
    id: number;
    productoId: number;
    nombreProducto: string;
    imagenUrl: string;
    precioUnitario: number;
    cantidad: number;
    subtotal: number;
    fechaAgregado: string;
}

export interface ProductFilter {
    busqueda?: string;
    categoriaId?: number;
    precioMin?: number;
    precioMax?: number;
    ordenarPor?: string;
}

export interface AddToCartRequest {
    productoId: number;
    cantidad: number;
}
