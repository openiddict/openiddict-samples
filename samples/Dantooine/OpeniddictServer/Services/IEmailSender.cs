using System.Threading.Tasks;

namespace OpeniddictServer.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
