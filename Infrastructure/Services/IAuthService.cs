using Application.DTOs.Auth;

namespace Infrastructure.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> AuthenticateAsync(LoginRequest request);

    }
}
