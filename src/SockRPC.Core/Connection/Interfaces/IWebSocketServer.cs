namespace SockRPC.Core.Connection.Interfaces;

public interface IWebSocketServer
{
    public Task HandleRequest(HttpContext context);
}