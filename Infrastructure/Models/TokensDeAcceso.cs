using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class TokensDeAcceso
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public bool Vigente { get; set; }

    public string? TipoToken { get; set; }

    public int FkUsuario { get; set; }

    public virtual Usuario FkUsuarioNavigation { get; set; } = null!;
}
