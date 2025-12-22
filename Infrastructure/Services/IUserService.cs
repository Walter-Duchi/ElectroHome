using Application.DTOs.User;

namespace Infrastructure.Services
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, string currentUserRole);
        Task<bool> CanCreateRole(string creatorRole, string targetRole);
    }
}