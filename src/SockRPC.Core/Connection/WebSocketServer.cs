using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Core.Connection;

public class WebSocketServer(IWebSocketConnectionAcceptor connectionAcceptor, ILogger<WebSocketServer> logger)
    : IWebSocketServer
{
    public async Task HandleRequest(HttpContext context)
    {
        try
        {
            // TODO: Add Processor 
            using var webSocket = await connectionAcceptor.AcceptConnectionAsync(context);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling WebSocket request");
        }
    }
}