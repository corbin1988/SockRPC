using System.Text.Json;
using System.Text.Json.Serialization;

namespace SockRPC.Core.JsonRpc;

public class JsonRpcResponse
{
    public JsonRpcResponse(string jsonrpc, string? id)
    {
        Jsonrpc = jsonrpc;
        Id = id;
    }

    [JsonPropertyName("jsonrpc")] public string Jsonrpc { get; set; } = "2.0";

    [JsonPropertyName("result")] public JsonElement? Result { get; set; }

    [JsonPropertyName("error")] public JsonRpcError? Error { get; set; }

    [JsonPropertyName("id")] public string? Id { get; set; }
}