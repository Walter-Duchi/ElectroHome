using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName);
    }
}