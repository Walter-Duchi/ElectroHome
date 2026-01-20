using Application.DTOs.Reclamos.User;

namespace Infrastructure.Reclamos.Interfaces
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, string currentUserRole);
        Task<bool> CanCreateRole(string creatorRole, string targetRole);
    }
}