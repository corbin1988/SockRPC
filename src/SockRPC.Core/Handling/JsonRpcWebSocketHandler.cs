using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SockRPC.Core.Handling.Interfaces;
using SockRPC.Core.JsonRpc;
using SockRPC.Core.JsonRpc.Interfaces;
using SockRPC.Core.Testing;

namespace SockRPC.Core.Handling;

public class JsonRpcWebSocketHandler(IJsonRpcRequestParser requestParser, ILogger<JsonRpcWebSocketHandler> logger)
    : IWebSocketMessageHandler
{
    public async Task HandleMessageAsync(WebSocket webSocket, WebSocketReceiveResult result, byte[] buffer)
    {
        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

        try
        {
            logger.LogInformation("Received message: {Message}", message);

            var request = requestParser.ParseAndValidate(message);

            var context = new JsonRpcContext(request, webSocket);

            await TestAcknowledgmentHelper.SendAcknowledgmentIfTest(webSocket, request);

            //TODO: Execute the route associated with the request
        }
        catch (JsonRpcValidationException ex)
        {
            logger.LogWarning(ex, "Validation error occurred while processing message: {Message}", message);
            await SendErrorResponse(webSocket, ex.ErrorResponse);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "JSON parsing error occurred while processing message: {Message}", message);
            var errorResponse = CreateErrorResponse(-32700, "Parse error: Invalid JSON.");
            await SendErrorResponse(webSocket, errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while processing message: {Message}", message);
            var errorResponse = CreateErrorResponse(-32603, "Internal error");
            await SendErrorResponse(webSocket, errorResponse);
        }
    }

    private async Task SendErrorResponse(WebSocket webSocket, JsonRpcResponse errorResponse)
    {
        var responseMessage = JsonSerializer.Serialize(errorResponse);
        var buffer = Encoding.UTF8.GetBytes(responseMessage);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }

    private JsonRpcResponse CreateErrorResponse(int code, string message)
    {
        return new JsonRpcResponse("2.0", null)
        {
            Error = new JsonRpcError(message, null) { Code = code }
        };
    }
}