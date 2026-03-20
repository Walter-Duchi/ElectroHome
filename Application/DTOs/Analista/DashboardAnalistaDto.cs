namespace Application.DTOs.Analista;

public class DashboardAnalistaDto
{
    public ResumenGeneralDto Resumen { get; set; } = new();
    public VentasPorPeriodoDto VentasUltimos30Dias { get; set; } = new();
    public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; } = new();
    public List<CategoriaVentasDto> VentasPorCategoria { get; set; } = new();
    public List<ReclamoEstadoDto> ReclamosPorEstado { get; set; } = new();
    public InventarioResumenDto Inventario { get; set; } = new();
    public UsuariosActivosDto Usuarios { get; set; } = new();
}

public class ResumenGeneralDto
{
    public decimal TotalIngresos { get; set; }
    public int TotalVentas { get; set; }
    public decimal PromedioVenta { get; set; }
    public int TotalProductosVendidos { get; set; }
    public int ProductosEnInventario { get; set; }
    public int ReclamosPendientes { get; set; }
    public int UsuariosActivos { get; set; }
}

public class VentasPorPeriodoDto
{
    public List<VentaDiariaDto> VentasDiarias { get; set; } = new();
    public decimal TotalPeriodo { get; set; }
    public int CantidadVentasPeriodo { get; set; }
    public decimal VariacionPorcentual { get; set; }
}

public class VentaDiariaDto
{
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public int CantidadVentas { get; set; }
}

public class ProductoMasVendidoDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public int UnidadesVendidas { get; set; }
    public decimal IngresoGenerado { get; set; }
}

public class CategoriaVentasDto
{
    public int CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public int UnidadesVendidas { get; set; }
    public decimal IngresoGenerado { get; set; }
    public decimal PorcentajeVentas { get; set; }
}

public class ReclamoEstadoDto
{
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
}

public class InventarioResumenDto
{
    public int TotalProductos { get; set; }
    public int StockDisponible { get; set; }
    public int StockPorUbicacion { get; set; }
    public List<ProductoBajoStockDto> ProductosBajoStock { get; set; } = new();
}

public class ProductoBajoStockDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int UmbralMinimo { get; set; }
}

public class UsuariosActivosDto
{
    public int Total { get; set; }
    public List<UsuariosPorRolDto> PorRol { get; set; } = new();
    public int NuevosUltimoMes { get; set; }
}

public class UsuariosPorRolDto
{
    public string Rol { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}