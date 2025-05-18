using System.Net.WebSockets;

namespace SockRPC.Core.Connection.Interfaces;

public interface IWebSocketConnectionAcceptor
{
    public Task<WebSocket> AcceptConnectionAsync(HttpContext context);
    public Task HandleDisconnectAsync(WebSocket webSocket);
}