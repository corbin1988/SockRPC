using SockRPC.Core.JsonRpc;

namespace SockRPC.Core.Middleware.Interfaces;

public interface IWebSocketMiddleware
{
    Task Handle(JsonRpcContext context, Func<Task> next);
}