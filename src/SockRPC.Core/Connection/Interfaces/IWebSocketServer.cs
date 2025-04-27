using System.Net.WebSockets;

namespace SockRPC.Core.Connection.Interfaces;

public interface IWebSocketServer
{
    public Task HandleRequest(HttpContext context);
    public Task Process(WebSocket webSocket);
}