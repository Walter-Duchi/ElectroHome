namespace Application.DTOs.Productos;

public class MarcaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CreateMarcaRequest
{
    public string Nombre { get; set; } = string.Empty;
}

public class UpdateMarcaRequest
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}