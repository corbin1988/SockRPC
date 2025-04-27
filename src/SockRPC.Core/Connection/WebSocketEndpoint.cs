using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Core.Connection;

public static class WebSocketEndpoint
{
    public static void MapWebSocketEndpoint(this IEndpointRouteBuilder app, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");

        app.Map(path, context =>
        {
            var webSocketServer = context.RequestServices.GetRequiredService<IWebSocketServer>();
            return webSocketServer.HandleRequest(context);
        });
    }
}