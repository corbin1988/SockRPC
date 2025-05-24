using System.Text.Json;
using SockRPC.Core.JsonRpc.Interfaces;

namespace SockRPC.Core.JsonRpc;

public class JsonRpcRequestParser(IJsonRpcValidator validator) : IJsonRpcRequestParser
{
    public JsonRpcRequest ParseAndValidate(string message)
    {
        var request = Parse(message);

        var validationResponse = validator.ValidateRequest(request);
        if (validationResponse != null) throw new JsonRpcValidationException(validationResponse);

        return request;
    }

    internal JsonRpcRequest Parse(string message)
    {
        try
        {
            var request = JsonSerializer.Deserialize<JsonRpcRequest>(message, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null)
                throw new JsonRpcException(-32700, "Parse error: Invalid JSON.");

            return request;
        }
        catch (JsonException)
        {
            throw new JsonRpcException(-32700, "Parse error: Invalid JSON.");
        }
    }
}