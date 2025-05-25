namespace SockRPC.Core.JsonRpc.Interfaces;

public interface IJsonRpcValidator
{
    JsonRpcResponse ValidateRequest(JsonRpcRequest request);
    JsonRpcResponse MethodNotFound(string requestMethod, string requestId);
    JsonRpcResponse CreateErrorResponse(int code, string internalError, string requestId);
}