using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Venta
{
    public int Id { get; set; }

    public string CodigoFactura { get; set; } = null!;

    public int FkEmpresaCliente { get; set; }

    public int? FkVendedor { get; set; }

    public string? TipoVenta { get; set; }

    public DateTime? FechaCompra { get; set; }

    public string? EstadoSri { get; set; }

    public string? ClaveAcceso { get; set; }

    public string? NumeroAutorizacion { get; set; }

    public DateTime? FechaAutorizacion { get; set; }

    public string? XmlPath { get; set; }

    public string? PdfPath { get; set; }

    public string? Observaciones { get; set; }

    public string? DireccionEntrega { get; set; }

    public string? TelefonoContacto { get; set; }

    public decimal TotalCompra { get; set; }

    public int? CreadoPor { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? ModificadoPor { get; set; }

    public virtual Usuario? CreadoPorNavigation { get; set; }

    public virtual Usuario FkEmpresaClienteNavigation { get; set; } = null!;

    public virtual Usuario? FkVendedorNavigation { get; set; }

    public virtual Usuario? ModificadoPorNavigation { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual ICollection<VentasPorNumeroSerieProducto> VentasPorNumeroSerieProductos { get; set; } = new List<VentasPorNumeroSerieProducto>();
}
