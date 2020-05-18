using System.Threading.Tasks;

namespace Velusia.Server.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
