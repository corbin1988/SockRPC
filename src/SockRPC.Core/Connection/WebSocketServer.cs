using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Core.Connection;

public class WebSocketServer(
    IWebSocketConnectionAcceptor connectionAcceptor,
    ILogger<WebSocketServer> logger,
    IWebSocketMessageProcessor messageProcessor)
    : IWebSocketServer
{
    public async Task HandleRequest(HttpContext context)
    {
        Console.WriteLine("Handling WebSocket request");
        try
        {
            using var webSocket = await connectionAcceptor.AcceptConnectionAsync(context);
            await messageProcessor.ProcessMessageAsync(webSocket, new byte[1024]);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling WebSocket request");
        }
    }
}