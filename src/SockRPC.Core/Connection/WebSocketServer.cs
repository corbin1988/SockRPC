using System.Net.WebSockets;
using SockRPC.Core.Configuration;
using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Core.Connection;

public class WebSocketServer(
    IWebSocketConnectionAcceptor connectionAcceptor,
    ILogger<WebSocketServer> logger,
    IWebSocketMessageProcessor messageProcessor,
    IWebSocketBufferManager bufferManager,
    WebSocketSettings settings)
    : IWebSocketServer
{
    public async Task HandleRequest(HttpContext context)
    {
        Console.WriteLine("Handling WebSocket request");
        try
        {
            using var webSocket = await connectionAcceptor.AcceptConnectionAsync(context);
            await Process(webSocket);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling WebSocket request");
        }
    }

    //TODO: Public for testing (can later change to internal + InternalsVisibleTo)
    internal async Task Process(WebSocket webSocket)
    {
        var buffer = bufferManager.RentBuffer(settings.BufferSize);
        try
        {
            Console.WriteLine("Client connected");

            while (webSocket.State == WebSocketState.Open)
                await messageProcessor.ProcessMessageAsync(webSocket, buffer, settings);
        }
        catch (WebSocketException)
        {
            await connectionAcceptor.HandleDisconnectAsync(webSocket);
        }
        finally
        {
            bufferManager.ReturnBuffer(buffer);
        }
    }
}