using System.Threading.Tasks;
using MessagingServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace MessagingServer.Services
{
    public class NotificationService : INotificationService
    {
        private const string clientCallback = "receiveMessage";
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly IConnectionMultiplexer _redis;

        public NotificationService(
            IHubContext<MessageHub> hubContext,
            IConnectionMultiplexer redis)
        {
            _hubContext = hubContext;
            _redis = redis;
        }
        public async Task AddSubscriberAsync(string subscriberId, string topic)
        {
            await _redis.GetSubscriber().SubscribeAsync(topic, async (channel, value) => 
            {
                await _hubContext.Clients.Client(subscriberId).SendAsync(clientCallback, value);
            });
        }

        public async Task PublishAsync(string topic, string message)
        {
            await _redis.GetSubscriber().PublishAsync(topic, message);
        }
    }
}