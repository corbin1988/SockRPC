namespace SockRPC.Core.JsonRpc.Interfaces;

public interface IJsonRpcValidator
{
    JsonRpcResponse ValidateRequest(JsonRpcRequest request);
}