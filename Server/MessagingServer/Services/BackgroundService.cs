using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessagingServer.Services
{
    public class BackgroundService : IBackgroundService
    {
        private const int DelaySeconds = 5;
        private INotificationService _notify;

        public BackgroundService(INotificationService notify)
        {
            _notify = notify;
        }

        public async Task SendMessageAsync(string subscriberId, string message)
        {
            // Add subscriber
            var sessionId = Guid.NewGuid().ToString();
            await _notify.AddSubscriberAsync(subscriberId, sessionId);

            // Simulate long-running task
            var timer = new Timer(async state =>
            {
                await _notify.PublishAsync(sessionId, message);
            }, message, DelaySeconds * 1000, Timeout.Infinite);
        }
    }
}