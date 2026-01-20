using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Reclamos.User
{
    public class CreateUserRequest
    {
        [Required]
        public string Nombres { get; set; } = null!;

        [Required]
        public string Apellidos { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = null!;

        [Required]
        public string Contrasena { get; set; } = null!;

        [Required]
        public string Celular { get; set; } = null!;

        public string? Convencional { get; set; }

        [Required]
        public string RUC { get; set; } = null!;

        [Required]
        public string Rol { get; set; } = null!;

        public string? NumCuentaBancaria { get; set; }

        public string? TipoCuentaBancaria { get; set; }
    }
}