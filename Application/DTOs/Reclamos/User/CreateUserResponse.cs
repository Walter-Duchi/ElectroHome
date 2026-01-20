namespace Application.DTOs.Reclamos.User
{
    public class CreateUserResponse
    {
        public int Id { get; set; }
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Celular { get; set; } = null!;
        public string Rol { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
    }
}