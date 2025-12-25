using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName);
    }
}