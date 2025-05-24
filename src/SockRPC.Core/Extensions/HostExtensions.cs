using SockRPC.Core.Configuration;
using SockRPC.Core.Connection;
using SockRPC.Core.Connection.Interfaces;
using SockRPC.Core.Handling;
using SockRPC.Core.Handling.Interfaces;
using SockRPC.Core.JsonRpc;
using SockRPC.Core.JsonRpc.Interfaces;

namespace SockRPC.Core.Extensions;

public static class HostExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var webSocketSettings = configuration.GetSection("WebSocketSettings").Get<WebSocketSettings>();

        if (webSocketSettings != null) services.AddSingleton(webSocketSettings);
        services.AddLogging();
        services.AddSingleton<IWebSocketServer, WebSocketServer>();
        services.AddSingleton<IWebSocketConnectionAcceptor, WebSocketConnectionAcceptor>();
        services.AddSingleton<IWebSocketHandlerMessageDispatcher, WebSocketMessageHandlerMessageDispatcher>();
        services.AddSingleton<JsonRpcWebSocketHandler>();
        services.AddSingleton<IWebSocketMessageProcessor, WebSocketMessageProcessor>();
        services.AddSingleton<IWebSocketBufferManager, WebSocketBufferManager>();
        services.AddSingleton<IJsonRpcRequestParser, JsonRpcRequestParser>();
        services.AddSingleton<IJsonRpcValidator, JsonRpcValidator>();
        services.AddSingleton<RawWebSocketMessageHandler>();
        services.AddSingleton<JsonRpcWebSocketHandler>();
    }

    public static void ConfigureMiddleware(this WebApplication app)
    {
        app.UseWebSockets();
        app.MapWebSocketEndpoint("/ws");
    }
}