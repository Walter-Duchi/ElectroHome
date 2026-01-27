using Application.DTOs.Auth;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> AuthenticateAsync(LoginRequest request);
        Task<bool> RequestPasswordResetAsync(string correo);
        Task<bool> ValidateResetTokenAsync(string token);
        Task<bool> ResetPasswordAsync(string token, string nuevaContrasena);
    }
}