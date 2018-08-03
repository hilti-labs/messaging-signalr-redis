using System.Threading.Tasks;

namespace MessagingServer.Services
{
    public interface IBackgroundService
    {
        Task SendMessageAsync(string connectionId, string message);
    }
}