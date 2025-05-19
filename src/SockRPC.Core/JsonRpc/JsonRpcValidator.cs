using System.Text.Json;
using SockRPC.Core.JsonRpc.Interfaces;

namespace SockRPC.Core.JsonRpc;

public class JsonRpcValidator : IJsonRpcValidator
{
    public JsonRpcResponse ValidateRequest(JsonRpcRequest request)
    {
        var versionResponse = ValidateJsonRpcVersion(request.Jsonrpc);
        if (versionResponse != null) return versionResponse;

        var methodResponse = ValidateMethod(request.Method);
        if (methodResponse != null) return methodResponse;

        return ValidateParams(request.Params);
    }

    public JsonRpcResponse MethodNotFound(string method)
    {
        return CreateErrorResponse(-32601, $"Method not found: {method}");
    }

    internal JsonRpcResponse ValidateJsonRpcVersion(string jsonrpc)
    {
        if (jsonrpc != "2.0") return CreateErrorResponse(-32600, "Invalid JSON-RPC version.");

        return null!;
    }

    internal JsonRpcResponse ValidateMethod(string method)
    {
        var methodParts = method.Split('.');
        if (methodParts.Length != 2)
            return CreateErrorResponse(-32601, "Method not found: Expected 'topic.action' format.");

        return null!;
    }

    internal JsonRpcResponse ValidateParams(JsonElement parameters)
    {
        if (parameters.ValueKind == JsonValueKind.Null)
            return CreateErrorResponse(-32602, "Invalid params: Parameters cannot be null.");

        return null!;
    }

    private JsonRpcResponse CreateErrorResponse(int code, string message)
    {
        return new JsonRpcResponse("2.0", null)
        {
            Error = new JsonRpcError(message, null)
            {
                Code = code
            }
        };
    }
}