namespace SockRPC.Core.JsonRpc;

public class JsonRpcValidationException(JsonRpcResponse errorResponse) : Exception
{
    public JsonRpcResponse ErrorResponse { get; } = errorResponse;
}