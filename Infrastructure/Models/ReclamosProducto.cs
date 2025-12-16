using System;
using System.Collections.Generic;

namespace Infrastructure.Models;

public partial class ReclamosProducto
{
    public int Id { get; set; }

    public int FkComprasProductos { get; set; }

    public int FkReclamos { get; set; }

    public DateTime FechaVentaClienteFinal { get; set; }

    public string? Estado { get; set; }

    public int? FkTecnicoAsignado { get; set; }

    public string? FormaCompensacion { get; set; }

    public DateTime? FechaRevisionTecnico { get; set; }

    public string? ExplicacionRespuestaTecnico { get; set; }

    public string? PdfRevisionTecnico { get; set; }

    public DateTime? FechaNotificacionLeida { get; set; }

    public virtual EvidenciaReemplazoReclamosProducto? EvidenciaReemplazoReclamosProducto { get; set; }

    public virtual ComprasProducto FkComprasProductosNavigation { get; set; } = null!;

    public virtual Reclamo FkReclamosNavigation { get; set; } = null!;

    public virtual Usuario? FkTecnicoAsignadoNavigation { get; set; }

    public virtual ReembolsoReclamosProducto? ReembolsoReclamosProducto { get; set; }
}
