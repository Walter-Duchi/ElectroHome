namespace Application.DTOs.Productos;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public int? CategoriaPadreId { get; set; }
    public string? CategoriaPadreNombre { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CreateCategoriaRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public int? CategoriaPadreId { get; set; }
}

public class UpdateCategoriaRequest
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public int? CategoriaPadreId { get; set; }
}