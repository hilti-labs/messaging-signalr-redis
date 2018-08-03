using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace NetCoreClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press Ctrl+C to terminate.");

            const string connectionIdEndpoint = "GetConnectionId";
            const string clientCallback = "receiveMessage";
            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/hubs/message")
                .Build();
            connection.On<string>(clientCallback, message =>
            {
                Console.WriteLine($"Received: {message}");
            });
            await connection.StartAsync();
            var connectionId = await connection.InvokeAsync<string>(connectionIdEndpoint);

            while (true)
            {
                Console.WriteLine("Message:");
                var message = Console.ReadLine();
                await Task.Run(async () =>
                {
                    using(var client = new HttpClient())
                    {
                        await client.GetAsync($"https://localhost:5001/api/message/{connectionId}/{message}");
                    }
                });
            }
        }
    }
}
