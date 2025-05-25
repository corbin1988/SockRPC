using System.Net.WebSockets;
using System.Text.Json;
using SockRPC.Core.Middleware.Interfaces;

namespace SockRPC.Core.Routing.Interfaces;

public interface IRouteRegistry
{
    void Register(string method, Func<JsonElement, string, WebSocket, Task> handler,
        List<IWebSocketMiddleware> middleware = null);

    RouteInfo? GetRoute(string method);
}