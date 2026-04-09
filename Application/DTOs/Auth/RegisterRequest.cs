using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Los nombres son obligatorios")]
        public string Nombres { get; set; } = null!;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        public string Apellidos { get; set; } = null!;

        [Required(ErrorMessage = "El tipo de identificación es obligatorio")]
        [RegularExpression("^(Cedula|Pasaporte)$", ErrorMessage = "Tipo de identificación inválido")]
        public string TipoIdentificacion { get; set; } = null!;

        [Required(ErrorMessage = "La identificación es obligatoria")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "La identificación debe tener entre 10 y 13 caracteres")]
        public string Identificacion { get; set; } = null!;

        public string? Ruc { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El celular es obligatorio")]
        [Phone(ErrorMessage = "Formato de celular inválido")]
        public string Celular { get; set; } = null!;

        public string? Convencional { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        public string Ciudad { get; set; } = null!;

        [Required(ErrorMessage = "El código postal es obligatorio")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Código postal inválido (6 dígitos)")]
        public string CodigoPostal { get; set; } = null!;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Contrasena { get; set; } = null!;
    }
}