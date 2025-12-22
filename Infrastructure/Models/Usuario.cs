using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Usuario
    {
        public Usuario()
        {
            Reclamos = new HashSet<Reclamo>();
            ReclamosProductoSns = new HashSet<ReclamosProductoSn>();
            UsuariosCertificacionMarcas = new HashSet<UsuariosCertificacionMarca>();
            Venta = new HashSet<Venta>();
        }

        public int Id { get; set; }
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public byte[] Contrasena { get; set; } = null!;
        public string Celular { get; set; } = null!;
        public string? Convencional { get; set; }
        public string Ruc { get; set; } = null!;
        public string Rol { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public string? NumCuentaBancaria { get; set; }

        public virtual ICollection<Reclamo> Reclamos { get; set; }
        public virtual ICollection<ReclamosProductoSn> ReclamosProductoSns { get; set; }
        public virtual ICollection<UsuariosCertificacionMarca> UsuariosCertificacionMarcas { get; set; }
        public virtual ICollection<Venta> Venta { get; set; }
    }
}