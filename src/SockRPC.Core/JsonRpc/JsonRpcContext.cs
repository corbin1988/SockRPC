using System.Net.WebSockets;

namespace SockRPC.Core.JsonRpc;

public class JsonRpcContext(JsonRpcRequest request, WebSocket socket, HttpContext? httpContext = null)
{
    public JsonRpcRequest Request { get; } = request;
    public WebSocket WebSocket { get; } = socket;
    public HttpContext? HttpContext { get; } = httpContext;
}