using System.Threading.Tasks;

namespace OpeniddictServer.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
