using SockRPC.Core.Configuration;
using SockRPC.Core.Connection;
using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Core.Extensions;

public static class HostExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var webSocketSettings = configuration.GetSection("WebSocketSettings").Get<WebSocketSettings>();

        if (webSocketSettings != null) services.AddSingleton(webSocketSettings);
        services.AddSingleton<IWebSocketServer, WebSocketServer>();
        services.AddSingleton<IWebSocketConnectionAcceptor, WebSocketConnectionAcceptor>();
        services.AddSingleton<IWebSocketMessageProcessor, WebSocketMessageProcessor>();
        services.AddSingleton<IWebSocketBufferManager, WebSocketBufferManager>();
    }

    public static void ConfigureMiddleware(this WebApplication app)
    {
        app.UseWebSockets();
        app.MapWebSocketEndpoint("/ws");
    }
}