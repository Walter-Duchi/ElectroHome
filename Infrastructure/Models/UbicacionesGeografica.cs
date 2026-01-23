using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class UbicacionesGeografica
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Tipo { get; set; }

    public int? FkPadre { get; set; }

    public string? CodigoPostal { get; set; }

    public string? ZonaHoraria { get; set; }

    public virtual UbicacionesGeografica? FkPadreNavigation { get; set; }

    public virtual ICollection<UbicacionesGeografica> InverseFkPadreNavigation { get; set; } = new List<UbicacionesGeografica>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
