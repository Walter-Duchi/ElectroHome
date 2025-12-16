using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public byte[] Contrasena { get; set; } = null!;

    public string Celular { get; set; } = null!;

    public string? Convencional { get; set; }

    public string Ruc { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public DateTime? FechaCreacion { get; set; }

    public string NumCuentaBancaria { get; set; } = null!;

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    public virtual ICollection<Reclamo> Reclamos { get; set; } = new List<Reclamo>();

    public virtual ICollection<ReclamosProducto> ReclamosProductos { get; set; } = new List<ReclamosProducto>();

    public virtual ICollection<UsuariosCertificacionMarca> UsuariosCertificacionMarcas { get; set; } = new List<UsuariosCertificacionMarca>();
}
