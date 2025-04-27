using System.Net.WebSockets;
using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Core.Connection;

public class WebSocketMessageProcessor : IWebSocketMessageProcessor
{
    public async Task ProcessMessageAsync(WebSocket webSocket, byte[] buffer)
    {
        Console.WriteLine("Received message: ");
        WebSocketReceiveResult result;

        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
                throw new WebSocketException("WebSocket connection closed.");
            // TODO: Process the received message here (e.g., decode the buffer)
        } while (!result.EndOfMessage);
    }
}