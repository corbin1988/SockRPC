using System.Net.WebSockets;

namespace SockRPC.Core.Connection.Interfaces;

public interface IWebSocketMessageProcessor
{
    public Task ProcessMessageAsync(WebSocket webSocket, byte[] buffer);
}