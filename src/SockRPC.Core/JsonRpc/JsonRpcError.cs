using System.Text.Json.Serialization;

namespace SockRPC.Core.JsonRpc;

public class JsonRpcError(string message, string? data)
{
    [JsonPropertyName("code")] public int Code { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; } = message;

    [JsonPropertyName("data")] public string? Data { get; set; } = data;
}