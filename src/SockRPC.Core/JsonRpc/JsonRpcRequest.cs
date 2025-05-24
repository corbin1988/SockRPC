using System.Text.Json;
using System.Text.Json.Serialization;

namespace SockRPC.Core.JsonRpc;

[method: JsonConstructor]
public class JsonRpcRequest(string jsonrpc, string method, JsonElement @params, string id)
{
    [JsonPropertyName("jsonrpc")] public string Jsonrpc { get; } = jsonrpc;

    [JsonPropertyName("method")] public string Method { get; } = method;

    [JsonPropertyName("params")] public JsonElement Params { get; } = @params;

    [JsonPropertyName("id")] public string Id { get; } = id;
}