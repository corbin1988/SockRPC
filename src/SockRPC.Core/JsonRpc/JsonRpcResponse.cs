using System.Text.Json.Serialization;

namespace SockRPC.Core.JsonRpc;

public class JsonRpcResponse
{
    public JsonRpcResponse(string jsonrpc, string? id)
    {
        Jsonrpc = jsonrpc;
        Id = id;
    }

    [JsonPropertyName("jsonrpc")] public string Jsonrpc { get; set; }

    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("result")] public object? Result { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonRpcError? Error { get; set; }
}