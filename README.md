# SignalR Messaging Demo with Redis

### Demo of using Redis to build scalable messaging service with SignalR

This sample demonstrates how to use Redis pub-sub to scale out a service that uses SignalR for pushing real-time notifications to clients.

## Prerequisites
- Install [Docker](https://www.docker.com/community-edition)
- Install [.NET Core SDK](https://www.microsoft.com/net/download)

## Redis Basic Usage
- Run Redis in a Docker container
    - Press **Ctrl+C** to stop and remove container
    ```
    docker run --name my-redis -p 6379:6379 --rm redis
    ```
- Connect via redis-cli from two different terminal windows
    ```
    docker run -it --link my-redis:redis --rm redis redis-cli -h redis -p 6379
    ```
- Execute pub-sub commands
    - First terminal: `subscribe channel_one`
    - Second terminal: `publish channel_one "hello"`
    - You should see the message appear in the first terminal.

## Configure ASP.NET Core Web API to use Redis
- Scaffold a new ASP.NET Core Web API project
    - From **Server/MessageingServer** folder execute:  
  `dotnet new webapi`
- Add Redis client NuGet package
    - From terminal at this folder run:  
  `dotnet add package StackExchange.Redis`
- Add Redis configuration
    - Update **appsettings.json** by adding
    ```json
    "redis": "localhost"
    ```
    - Update `ConfigureServices` method in `Startup`.
    ```csharp
    var redisConnection = Configuration.GetSection("redis").Value;
    services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(redisConnection));
    ```

## Configure ASP.NET Core Web API to use SignalR
- Update `ConfigureServices` method in `Startup`.
    ```csharp
    services.AddSignalR();
    ```
- Update `Configure` method in `Startup`.
    ```csharp
    app.UseSignalR((options) => {
        options.MapHub<MessageHub>("/hubs/message");
    });
    ```
- Add **MessageHub.cs** to **Hubs** folder with method to return connection id.
    ```csharp
    public class MessageHub : Hub
    {
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
    ```
- Inject `IHubContext<MessageHub>` and `IConnectionMultiplexer` into the `NotificationService` class.
- Add a method that subscribes to a Redis topic and calls client with specific connection id.
    ```csharp
    public async Task AddSubscriberAsync(string subscriberId, string topic)
    {
        await _redis.GetSubscriber().SubscribeAsync(topic, async (channel, value) => 
        {
            await _hubContext.Clients.Client(subscriberId).SendAsync(clientCallback, value);
        });
    }
    ```
- Add a method to publish a message for a specific topic.
    ```csharp
    public async Task PublishAsync(string topic, string message)
    {
        await _redis.GetSubscriber().PublishAsync(topic, message);
    }
    ```
- Add a method to `BackgroundService` to simulate a long running task by adding a subscriber to the notification service.
    ```csharp
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
    ```

## Create a .NET Core client
- Create a .NET Core console application.
- Add package reference for **Microsoft.AspNetCore.SignalR.Client**
- Use `HubConnectionBuilder` to build a connection and handle a callback to get the connection id.
    ```csharp
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
    ```
- Add a `while` loop to send messages to the message service
    ```csharp
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
    ```
