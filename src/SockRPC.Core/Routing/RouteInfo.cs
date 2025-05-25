using System.Net.WebSockets;
using System.Text.Json;
using SockRPC.Core.Middleware.Interfaces;

namespace SockRPC.Core.Routing;

public class RouteInfo
{
    public Func<JsonElement, string, WebSocket, Task> Handler { get; set; }
    public List<IWebSocketMiddleware> WebSocketMiddleware { get; set; }
}