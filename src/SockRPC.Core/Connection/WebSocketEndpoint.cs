using System.Net;
using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Core.Connection;

public static class WebSocketEndpoint
{
    public static void MapWebSocketEndpoint(this IEndpointRouteBuilder app, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");

        app.Map("/ws", async context =>
        {
            Console.WriteLine("Request received at /ws");
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var webSocketServer = context.RequestServices.GetRequiredService<IWebSocketServer>();
            await webSocketServer.HandleRequest(context);
        });
    }
}