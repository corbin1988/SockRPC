using SockRPC.Core.JsonRpc;

namespace SockRPC.Core.Routing.Interfaces;

public interface IRouteExecutor
{
    Task ExecuteAsync(JsonRpcContext context);
}