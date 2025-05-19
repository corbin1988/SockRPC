using SockRPC.Core.Handling.Interfaces;

namespace SockRPC.Core.Handling;

public class WebSocketMessageHandlerMessageDispatcher(
    JsonRpcWebSocketHandler jsonRpcWebSocketHandler,
    RawWebSocketMessageHandler rawWebSocketMessageHandler) : IWebSocketHandlerMessageDispatcher
{
    public IWebSocketMessageHandler GetHandler(ReadOnlySpan<char> message)
    {
        if (message.IsEmpty)
            throw new InvalidOperationException("Message was empty");

        var trimmedMessage = message.TrimStart();

        return trimmedMessage.StartsWith("{") && trimmedMessage.EndsWith("}")
            ? jsonRpcWebSocketHandler
            : rawWebSocketMessageHandler;
    }
}