using System.Net.WebSockets;
using SockRPC.Core.Handling.Interfaces;

namespace SockRPC.Core.Handling;

public class JsonRpcWebSocketHandler : IWebSocketMessageHandler
{
    public Task HandleMessageAsync(WebSocket webSocket, WebSocketReceiveResult result, byte[] buffer)
    {
        throw new NotImplementedException();
    }
}