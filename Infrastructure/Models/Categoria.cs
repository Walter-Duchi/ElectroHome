using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class Categoria
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }

    public int? FkCategoriaPadre { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Categoria? FkCategoriaPadreNavigation { get; set; }

    public virtual ICollection<Categoria> InverseFkCategoriaPadreNavigation { get; set; } = new List<Categoria>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
