using System.Net.WebSockets;
using System.Text;
using SockRPC.Core.Handling.Interfaces;
using SockRPC.Core.Testing;

namespace SockRPC.Core.Handling;

public class RawWebSocketMessageHandler(ILogger<RawWebSocketMessageHandler> logger) : IWebSocketMessageHandler
{
    public async Task HandleMessageAsync(WebSocket webSocket, WebSocketReceiveResult result, byte[] buffer)
    {
        try
        {
            var receivedMessage = DecodeMessage(buffer, result.Count);
            //TODO: Will remove when production ready
            logger.LogInformation("Received message: {Message}", receivedMessage);

            await SendMessageAsync(webSocket, receivedMessage, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling WebSocket message");
        }
    }

    private static string DecodeMessage(byte[] buffer, int count)
    {
        return Encoding.UTF8.GetString(buffer, 0, count);
    }

    private static async Task SendMessageAsync(WebSocket webSocket, string message, CancellationToken cancellationToken)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
    }
}