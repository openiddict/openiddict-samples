using System.Threading.Tasks;

namespace AuthorizationServer.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
