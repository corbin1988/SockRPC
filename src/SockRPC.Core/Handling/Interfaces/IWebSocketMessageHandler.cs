using System.Net.WebSockets;

namespace SockRPC.Core.Handling.Interfaces;

public interface IWebSocketMessageHandler
{
    Task HandleMessageAsync(WebSocket webSocket, WebSocketReceiveResult result, byte[] buffer);
}