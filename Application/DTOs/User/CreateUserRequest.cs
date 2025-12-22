namespace Application.DTOs.User
{
    public class CreateUserRequest
    {
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Contrasena { get; set; } = null!;
        public string Celular { get; set; } = null!;
        public string? Convencional { get; set; }
        public string RUC { get; set; } = null!;
        public string Rol { get; set; } = null!;
        public string? NumCuentaBancaria { get; set; }
    }
}