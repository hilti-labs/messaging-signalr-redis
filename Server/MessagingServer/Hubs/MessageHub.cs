using Microsoft.AspNetCore.SignalR;

namespace MessagingServer.Hubs
{
    public class MessageHub : Hub
    {
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
