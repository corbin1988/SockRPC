using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SockRPC.Core.Handling.Interfaces;
using SockRPC.Core.JsonRpc;
using SockRPC.Core.JsonRpc.Interfaces;

namespace SockRPC.Core.Handling;

public class JsonRpcWebSocketHandler(IJsonRpcRequestParser requestParser) : IWebSocketMessageHandler
{
    public async Task HandleMessageAsync(WebSocket webSocket, WebSocketReceiveResult result, byte[] buffer)
    {
        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

        try
        {
            var request = requestParser.ParseAndValidate(message);

            var context = new JsonRpcContext(request, webSocket);

            //TODO: Execute the route associated with the request
        }
        catch (JsonRpcValidationException ex)
        {
            await SendErrorResponse(webSocket, ex.ErrorResponse);
        }
        catch (JsonException)
        {
            var errorResponse = CreateErrorResponse(-32700, "Parse error: Invalid JSON.");
            await SendErrorResponse(webSocket, errorResponse);
        }
        catch (Exception)
        {
            var errorResponse = CreateErrorResponse(-32603, "Internal error");
            await SendErrorResponse(webSocket, errorResponse);
        }
    }

    private async Task SendErrorResponse(WebSocket webSocket, JsonRpcResponse errorResponse)
    {
        var responseMessage = JsonSerializer.Serialize(errorResponse);
        var buffer = Encoding.UTF8.GetBytes(responseMessage);
        //TODO: Reuse RawWebSocketMessageHandler.SendMessageAsync
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