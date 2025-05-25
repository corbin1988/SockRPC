using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SockRPC.Core.JsonRpc;
using SockRPC.Core.JsonRpc.Interfaces;
using SockRPC.Core.Routing.Interfaces;

namespace SockRPC.Core.Routing;

public class RouteExecutor(IRouteRegistry registry, IJsonRpcValidator jsonRpcValidator) : IRouteExecutor
{
    public async Task ExecuteAsync(JsonRpcContext context)
    {
        var routeInfo = GetRouteInfo(context);
        if (routeInfo == null)
        {
            await HandleRouteNotFound(context);
            return;
        }

        try
        {
            await ExecuteMiddlewareChain(context, routeInfo, () => ExecuteHandler(context, routeInfo));
        }
        catch (Exception ex)
        {
            await HandleExecutionError(context, ex);
        }
    }

    private RouteInfo? GetRouteInfo(JsonRpcContext context)
    {
        var method = context.Request.Method;
        return registry.GetRoute(method);
    }

    private async Task HandleRouteNotFound(JsonRpcContext context)
    {
        var error = jsonRpcValidator.MethodNotFound(context.Request.Method, context.Request.Id);
        await SendError(context.WebSocket, error);
    }

    private Task ExecuteMiddlewareChain(JsonRpcContext context, RouteInfo routeInfo, Func<Task> finalHandler)
    {
        var pipeline = finalHandler;

        foreach (var middleware in routeInfo.WebSocketMiddleware.AsEnumerable().Reverse())
        {
            var next = pipeline;
            pipeline = () => middleware.Handle(context, next);
        }

        return pipeline();
    }

    private async Task ExecuteHandler(JsonRpcContext context, RouteInfo routeInfo)
    {
        await routeInfo.Handler(context.Request.Params, context.Request.Id, context.WebSocket);
    }

    private async Task HandleExecutionError(JsonRpcContext context, Exception ex)
    {
        var error = jsonRpcValidator.CreateErrorResponse(-32603, "Internal error: " + ex.Message, context.Request.Id);
        await SendError(context.WebSocket, error);
    }

    private async Task SendError(WebSocket webSocket, JsonRpcResponse response)
    {
        var responseMessage = JsonSerializer.Serialize(response);
        var buffer = Encoding.UTF8.GetBytes(responseMessage);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
}