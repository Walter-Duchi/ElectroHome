using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Productos;

public class ProductoManagementDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string NombreCompleto => $"{MarcaNombre} {Modelo}";
    public int MarcaId { get; set; }
    public string MarcaNombre { get; set; } = string.Empty;
    public int? CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public string Especificacion { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int DiasGarantia { get; set; }
    public string Visibilidad { get; set; } = "Publico";
    public bool Activo { get; set; }
    public decimal? PesoKg { get; set; }
    public decimal AltoCm { get; set; }
    public decimal AnchoCm { get; set; }
    public decimal ProfundidadCm { get; set; }
    public string? ImagenUrl { get; set; }
    public List<string> ImagenesAdicionales { get; set; } = new();
    public DateTime FechaCreacion { get; set; }
    public int? CreadoPor { get; set; }
    public int? ModificadoPor { get; set; }
}

public class CreateProductoRequest
{
    public string Sku { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public int MarcaId { get; set; }
    public int? CategoriaId { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Especificacion { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int DiasGarantia { get; set; }
    public string Visibilidad { get; set; } = "Publico";
    public decimal? PesoKg { get; set; }
    public decimal AltoCm { get; set; }
    public decimal AnchoCm { get; set; }
    public decimal ProfundidadCm { get; set; }
    public IFormFile? ImagenPrincipal { get; set; }
    public List<IFormFile>? ImagenesAdicionales { get; set; }
}

public class UpdateProductoRequest
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public int MarcaId { get; set; }
    public int? CategoriaId { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Especificacion { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int DiasGarantia { get; set; }
    public string Visibilidad { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public decimal? PesoKg { get; set; }
    public decimal AltoCm { get; set; }
    public decimal AnchoCm { get; set; }
    public decimal ProfundidadCm { get; set; }
    public IFormFile? ImagenPrincipal { get; set; }
    public List<IFormFile>? ImagenesAdicionales { get; set; }
    public List<string>? ImagenesAEliminar { get; set; }
}