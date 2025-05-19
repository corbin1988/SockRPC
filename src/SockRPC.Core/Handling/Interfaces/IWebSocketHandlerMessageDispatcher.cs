namespace SockRPC.Core.Handling.Interfaces;

public interface IWebSocketHandlerMessageDispatcher
{
    public IWebSocketMessageHandler GetHandler(ReadOnlySpan<char> message);
}