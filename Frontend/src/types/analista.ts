export interface DashboardAnalista {
    resumen: ResumenGeneral;
    ventasUltimos30Dias: VentasPorPeriodo;
    productosMasVendidos: ProductoMasVendido[];
    ventasPorCategoria: CategoriaVentas[];
    reclamosPorEstado: ReclamoEstado[];
    inventario: InventarioResumen;
    usuarios: UsuariosActivos;
}

export interface ResumenGeneral {
    totalIngresos: number;
    totalVentas: number;
    promedioVenta: number;
    totalProductosVendidos: number;
    productosEnInventario: number;
    reclamosPendientes: number;
    usuariosActivos: number;
}

export interface VentasPorPeriodo {
    ventasDiarias: VentaDiaria[];
    totalPeriodo: number;
    cantidadVentasPeriodo: number;
    variacionPorcentual: number;
}

export interface VentaDiaria {
    fecha: string;
    total: number;
    cantidadVentas: number;
}

export interface ProductoMasVendido {
    productoId: number;
    nombreProducto: string;
    marca: string;
    unidadesVendidas: number;
    ingresoGenerado: number;
}

export interface CategoriaVentas {
    categoriaId: number;
    nombreCategoria: string;
    unidadesVendidas: number;
    ingresoGenerado: number;
    porcentajeVentas: number;
}

export interface ReclamoEstado {
    estado: string;
    cantidad: number;
    porcentaje: number;
}

export interface InventarioResumen {
    totalProductos: number;
    stockDisponible: number;
    stockPorUbicacion: number;
    productosBajoStock: ProductoBajoStock[];
}

export interface ProductoBajoStock {
    productoId: number;
    nombreProducto: string;
    stockActual: number;
    umbralMinimo: number;
}

export interface UsuariosActivos {
    total: number;
    porRol: UsuariosPorRol[];
    nuevosUltimoMes: number;
}

export interface UsuariosPorRol {
    rol: string;
    cantidad: number;
}
