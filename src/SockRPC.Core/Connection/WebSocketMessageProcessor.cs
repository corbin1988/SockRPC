using System.Net.WebSockets;
using System.Text;
using SockRPC.Core.Configuration;
using SockRPC.Core.Connection.Interfaces;
using SockRPC.Core.Handling.Interfaces;

namespace SockRPC.Core.Connection;

public class WebSocketMessageProcessor(IWebSocketHandlerMessageDispatcher handlerMessageFactory)
    : IWebSocketMessageProcessor
{
    public async Task ProcessMessageAsync(WebSocket webSocket, byte[] buffer, WebSocketSettings settings)
    {
        var fullMessage = await ReadFullMessageAsync(webSocket, buffer, settings, CancellationToken.None);

        //TODO: Add a message handler to process the message here
        await HandleMessage(fullMessage, webSocket);
    }

    internal async Task<string> ReadFullMessageAsync(WebSocket webSocket, byte[] buffer, WebSocketSettings settings,
        CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream(settings.MaxMessageSize);
        WebSocketReceiveResult result;

        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
                throw new WebSocketException("WebSocket connection closed.");

            ms.Write(buffer, 0, result.Count);

            if (ms.Length > settings.MaxMessageSize)
                throw new InvalidOperationException("WebSocket message exceeded allowed size.");
        } while (!result.EndOfMessage);

        ms.Seek(0, SeekOrigin.Begin);
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    internal async Task HandleMessage(string fullMessage, WebSocket webSocket)
    {
        var handler = handlerMessageFactory.GetHandler(fullMessage);
        var fullBuffer = Encoding.UTF8.GetBytes(fullMessage);
        var finalResult = new WebSocketReceiveResult(fullBuffer.Length, WebSocketMessageType.Text, true);

        await handler.HandleMessageAsync(webSocket, finalResult, fullBuffer);
    }
}