using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SockRPC.Core.JsonRpc;

namespace SockRPC.Core.Testing;

public static class TestAcknowledgmentHelper
{
    public static bool IsTestEnvironment { get; set; } = false;

    public static async Task SendAcknowledgmentIfTest(WebSocket webSocket, JsonRpcRequest request)
    {
        if (!IsTestEnvironment) return;

        var acknowledgmentResponse = new JsonRpcResponse("2.0", request.Id)
        {
            Id = request.Id,
            Result = "Acknowledged"
        };

        var responseMessage = JsonSerializer.Serialize(acknowledgmentResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var buffer = Encoding.UTF8.GetBytes(responseMessage);
        Console.WriteLine($"TaskAcknowledgement Response: {responseMessage}");
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
}