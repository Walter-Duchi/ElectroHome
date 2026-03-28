namespace Application.DTOs.User
{
    public class UpdateProfileRequest
    {
        public string Correo { get; set; }
        public string Celular { get; set; }
        public string Convencional { get; set; }
        public string Ciudad { get; set; }
        public string CodigoPostal { get; set; }
        public string Direccion { get; set; }

        // Campos para cambio de contraseña (opcional)
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}