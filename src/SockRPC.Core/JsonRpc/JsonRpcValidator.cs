using System.Text.Json;
using SockRPC.Core.JsonRpc.Interfaces;

namespace SockRPC.Core.JsonRpc;

public class JsonRpcValidator : IJsonRpcValidator
{
    public JsonRpcResponse ValidateRequest(JsonRpcRequest request)
    {
        if (ValidateJsonRpcVersion(request.Jsonrpc, request.Id) is { } versionResponse) return versionResponse;

        if (ValidateMethod(request.Method, request.Id) is { } methodResponse) return methodResponse;

        return ValidateParams(request.Params, request.Id);
    }

    public JsonRpcResponse MethodNotFound(string method, string id)
    {
        return CreateErrorResponse(-32601, $"Method not found: {method}", id);
    }

    public JsonRpcResponse CreateErrorResponse(int code, string message, string id)
    {
        return new JsonRpcResponse("2.0", id)
        {
            Id = id,
            Error = new JsonRpcError(message, null)
            {
                Code = code
            }
        };
    }

    internal JsonRpcResponse ValidateJsonRpcVersion(string jsonrpc, string id)
    {
        if (jsonrpc != "2.0") return CreateErrorResponse(-32600, "Invalid JSON-RPC version.", id);

        return null!;
    }

    internal JsonRpcResponse ValidateMethod(string method, string id)
    {
        var methodParts = method.Split('.');
        if (methodParts.Length != 2)
            return CreateErrorResponse(-32601, "Method not found: Expected 'topic.action' format.", id);

        return null!;
    }

    internal JsonRpcResponse ValidateParams(JsonElement parameters, string id)
    {
        if (parameters.ValueKind == JsonValueKind.Null)
            return CreateErrorResponse(-32602, "Invalid params: Parameters cannot be null.", id);

        return null!;
    }
}