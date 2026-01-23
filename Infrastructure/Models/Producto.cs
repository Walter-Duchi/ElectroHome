using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Producto
{
    public int Id { get; set; }

    public string Sku { get; set; } = null!;

    public decimal? PesoKg { get; set; }

    public decimal AltoCm { get; set; }

    public decimal AnchoCm { get; set; }

    public decimal ProfundidadCm { get; set; }

    public string? Visibilidad { get; set; }

    public string Codigo { get; set; } = null!;

    public int FkMarca { get; set; }

    public int? FkCategoria { get; set; }

    public string Modelo { get; set; } = null!;

    public string Especificacion { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public string? ImagenUrl { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int FkImpuestoArancelario { get; set; }

    public int DiasGarantia { get; set; }

    public decimal Precio { get; set; }

    public int? CreadoPor { get; set; }

    public int? ModificadoPor { get; set; }

    public virtual ICollection<CarritoCompra> CarritoCompras { get; set; } = new List<CarritoCompra>();

    public virtual Usuario? CreadoPorNavigation { get; set; }

    public virtual Categoria? FkCategoriaNavigation { get; set; }

    public virtual ImpuestoArancelario FkImpuestoArancelarioNavigation { get; set; } = null!;

    public virtual Marca FkMarcaNavigation { get; set; } = null!;

    public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();

    public virtual Usuario? ModificadoPorNavigation { get; set; }

    public virtual ICollection<NumeroSerieProducto> NumeroSerieProductos { get; set; } = new List<NumeroSerieProducto>();

    public virtual ICollection<ProductoImagene> ProductoImagenes { get; set; } = new List<ProductoImagene>();

    public virtual ICollection<ProductoImpuesto> ProductoImpuestos { get; set; } = new List<ProductoImpuesto>();

    public virtual ICollection<ProductoPrecioHistorial> ProductoPrecioHistorials { get; set; } = new List<ProductoPrecioHistorial>();

    public virtual ProductosPopularesCache? ProductosPopularesCache { get; set; }

    public virtual ICollection<ResenasProducto> ResenasProductos { get; set; } = new List<ResenasProducto>();
}
