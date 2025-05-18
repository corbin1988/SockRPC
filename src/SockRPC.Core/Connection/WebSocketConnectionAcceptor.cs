using System.Net.WebSockets;
using Microsoft.AspNetCore.Http.Features;
using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Core.Connection;

public class WebSocketConnectionAcceptor : IWebSocketConnectionAcceptor
{
    public async Task<WebSocket> AcceptConnectionAsync(HttpContext context)
    {
        var webSocketFeature = context.Features.Get<IHttpWebSocketFeature>();

        if (webSocketFeature is null || !webSocketFeature.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new InvalidOperationException("Invalid WebSocket request.");
        }

        return await webSocketFeature.AcceptAsync(null);
    }

    public async Task HandleDisconnectAsync(WebSocket webSocket)
    {
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
    }
}