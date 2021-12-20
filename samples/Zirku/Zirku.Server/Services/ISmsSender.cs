using System.Threading.Tasks;

namespace Zirku.Server.Services;

public interface ISmsSender
{
    Task SendSmsAsync(string number, string message);
}
