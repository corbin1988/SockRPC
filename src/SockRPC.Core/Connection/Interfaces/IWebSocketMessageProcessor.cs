using System.Net.WebSockets;
using SockRPC.Core.Configuration;

namespace SockRPC.Core.Connection.Interfaces;

public interface IWebSocketMessageProcessor
{
    public Task ProcessMessageAsync(WebSocket webSocket, byte[] buffer, WebSocketSettings settings);
}