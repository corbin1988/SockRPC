using System.Net.WebSockets;
using System.Text.Json;
using SockRPC.Core.Middleware.Interfaces;
using SockRPC.Core.Routing.Interfaces;

namespace SockRPC.Core.Routing;

public class RouteRegistry : IRouteRegistry
{
    private readonly Dictionary<string, RouteInfo> _routes = new();

    public void Register(string method, Func<JsonElement, string, WebSocket, Task> handler,
        List<IWebSocketMiddleware> middleware = null)
    {
        _routes[method] = new RouteInfo
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler)),
            WebSocketMiddleware = middleware ?? new List<IWebSocketMiddleware>()
        };
    }

    public RouteInfo? GetRoute(string method)
    {
        return _routes.TryGetValue(method, out var info) ? info : null;
    }
}