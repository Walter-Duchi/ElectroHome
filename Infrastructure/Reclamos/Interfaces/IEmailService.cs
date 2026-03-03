namespace Infrastructure.Reclamos.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName);
    }
}