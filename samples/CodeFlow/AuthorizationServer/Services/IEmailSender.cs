using System.Threading.Tasks;

namespace AuthorizationServer.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
