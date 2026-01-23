using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string? RazonSocial { get; set; }

    public string? TipoIdentificacion { get; set; }

    public string Identificacion { get; set; } = null!;

    public string? Ruc { get; set; }

    public string Correo { get; set; } = null!;

    public byte[] Contrasena { get; set; } = null!;

    public string Celular { get; set; } = null!;

    public string? Convencional { get; set; }

    public string? Direccion { get; set; }

    public string? Ciudad { get; set; }

    public string Rol { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public string? NumCuentaBancaria { get; set; }

    public string? TipoCuentaBancaria { get; set; }

    public bool? Activo { get; set; }

    public bool? ContribuyenteEspecial { get; set; }

    public bool? ObligadoContabilidad { get; set; }

    public int? FkUbicacion { get; set; }

    public virtual ICollection<CarritoCompra> CarritoCompras { get; set; } = new List<CarritoCompra>();

    public virtual ICollection<ComprobanteDeReemplazo> ComprobanteDeReemplazos { get; set; } = new List<ComprobanteDeReemplazo>();

    public virtual ICollection<ConfiguracionGeneral> ConfiguracionGenerals { get; set; } = new List<ConfiguracionGeneral>();

    public virtual UbicacionesGeografica? FkUbicacionNavigation { get; set; }

    public virtual ICollection<ImpuestosConfiguracion> ImpuestosConfiguracionCreadoPorNavigations { get; set; } = new List<ImpuestosConfiguracion>();

    public virtual ICollection<ImpuestosConfiguracion> ImpuestosConfiguracionModificadoPorNavigations { get; set; } = new List<ImpuestosConfiguracion>();

    public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();

    public virtual ICollection<Producto> ProductoCreadoPorNavigations { get; set; } = new List<Producto>();

    public virtual ICollection<Producto> ProductoModificadoPorNavigations { get; set; } = new List<Producto>();

    public virtual ICollection<ProductoPrecioHistorial> ProductoPrecioHistorials { get; set; } = new List<ProductoPrecioHistorial>();

    public virtual ICollection<Reclamo> Reclamos { get; set; } = new List<Reclamo>();

    public virtual ICollection<ReclamosProductoSn> ReclamosProductoSns { get; set; } = new List<ReclamosProductoSn>();

    public virtual ICollection<Reembolso> Reembolsos { get; set; } = new List<Reembolso>();

    public virtual ICollection<ResenasProducto> ResenasProductos { get; set; } = new List<ResenasProducto>();

    public virtual ICollection<TokensDeAcceso> TokensDeAccesos { get; set; } = new List<TokensDeAcceso>();

    public virtual ICollection<UsuariosCertificacionMarca> UsuariosCertificacionMarcas { get; set; } = new List<UsuariosCertificacionMarca>();

    public virtual ICollection<Venta> VentaCreadoPorNavigations { get; set; } = new List<Venta>();

    public virtual ICollection<Venta> VentaFkEmpresaClienteNavigations { get; set; } = new List<Venta>();

    public virtual ICollection<Venta> VentaFkVendedorNavigations { get; set; } = new List<Venta>();

    public virtual ICollection<Venta> VentaModificadoPorNavigations { get; set; } = new List<Venta>();
}
