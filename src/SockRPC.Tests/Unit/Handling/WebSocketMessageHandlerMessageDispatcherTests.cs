using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SockRPC.Core.Handling;
using SockRPC.Core.JsonRpc.Interfaces;
using SockRPC.Core.Routing.Interfaces;

namespace SockRPC.Tests.Unit.Handling;

[TestFixture]
public class WebSocketMessageHandlerMessageDispatcherTests
{
    [SetUp]
    public void SetUp()
    {
        var jsonRpcLogger = Substitute.For<ILogger<JsonRpcWebSocketHandler>>();
        var rawWebSocketLogger = Substitute.For<ILogger<RawWebSocketMessageHandler>>();
        var requestParser = Substitute.For<IJsonRpcRequestParser>();
        var routeExecutor = Substitute.For<IRouteExecutor>();

        _jsonRpcWebSocketHandler = new JsonRpcWebSocketHandler(requestParser, routeExecutor, jsonRpcLogger);
        _rawWebSocketMessageHandler = new RawWebSocketMessageHandler(rawWebSocketLogger);

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