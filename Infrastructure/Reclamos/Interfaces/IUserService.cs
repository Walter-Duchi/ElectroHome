using Application.DTOs.Reclamos.User;
using Application.DTOs.User;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, int creadoPorId);
        Task<bool> CanCreateRole(string creatorRole, string targetRole);
        Task<ProfileResponse> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    }
}