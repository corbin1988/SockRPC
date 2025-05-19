using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SockRPC.Core.Handling;
using SockRPC.Core.JsonRpc.Interfaces;

namespace SockRPC.Tests.Unit.Handling;

[TestFixture]
public class WebSocketMessageHandlerMessageDispatcherTests
{
    [SetUp]
    public void SetUp()
    {
        var requestParser = Substitute.For<IJsonRpcRequestParser>();
        _jsonRpcWebSocketHandler = new JsonRpcWebSocketHandler(requestParser);

        var logger = Substitute.For<ILogger<RawWebSocketMessageHandler>>();
        _rawWebSocketMessageHandler = new RawWebSocketMessageHandler(logger);

        _messageDispatcher =
            new WebSocketMessageHandlerMessageDispatcher(_jsonRpcWebSocketHandler, _rawWebSocketMessageHandler);
    }

    private JsonRpcWebSocketHandler _jsonRpcWebSocketHandler;
    private RawWebSocketMessageHandler _rawWebSocketMessageHandler;
    private WebSocketMessageHandlerMessageDispatcher _messageDispatcher;

    [Test]
    public void Given_JsonRpcMessage_When_GetHandlerIsCalled_Then_ReturnsJsonRpcHandler()
    {
        // Given
        var message = "{ \"jsonrpc\": \"2.0\", \"method\": \"test\" }".AsSpan();

        // When
        var handler = _messageDispatcher.GetHandler(message);

        // Then
        handler.Should().Be(_jsonRpcWebSocketHandler);
    }

    [Test]
    public void Given_RawMessage_When_GetHandlerIsCalled_Then_ReturnsRawHandler()
    {
        // Given
        var message = "RAW MESSAGE".AsSpan();

        // When
        var handler = _messageDispatcher.GetHandler(message);

        // Then
        handler.Should().Be(_rawWebSocketMessageHandler);
    }

    [Test]
    public void Given_EmptyMessage_When_GetHandlerIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Given
        var message = "".AsSpan();
        var messageString = message.ToString();

        // When/Then
        FluentActions.Invoking(() => _messageDispatcher.GetHandler(messageString.AsSpan()))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Message was empty");
    }
}