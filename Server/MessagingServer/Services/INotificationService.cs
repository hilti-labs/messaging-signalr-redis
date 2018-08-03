using System.Threading.Tasks;

namespace MessagingServer.Services
{
    public interface INotificationService
    {
        Task AddSubscriberAsync(string subscriberId, string topic);
        Task PublishAsync(string topic, string message);
    }
}