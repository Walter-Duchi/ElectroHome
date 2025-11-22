using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class Marca
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<UsuariosCertificacionMarca> UsuariosCertificacionMarcas { get; set; } = new List<UsuariosCertificacionMarca>();
}
