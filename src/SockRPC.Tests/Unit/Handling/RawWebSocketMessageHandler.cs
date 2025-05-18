using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SockRPC.Core.Handling;

namespace SockRPC.Tests.Unit.Handling;

[TestFixture]
public class RawWebSocketMessageHandlerTests
{
    private ILogger<RawWebSocketMessageHandler> _logger;
    private RawWebSocketMessageHandler _handler;

    [SetUp]
    public void SetUp()
    {
        // Given: A mocked logger and a RawWebSocketMessageHandler instance
        _logger = Substitute.For<ILogger<RawWebSocketMessageHandler>>();
        _handler = new RawWebSocketMessageHandler(_logger);
    }

    [Test]
    public async Task HandleMessageAsync_WhenMessageIsReceived_ShouldLogAndSendMessage()
    {
        // Given: A WebSocket, a message, and a buffer
        var webSocket = Substitute.For<WebSocket>();
        var receivedMessage = "Hello, WebSocket!";
        var buffer = Encoding.UTF8.GetBytes(receivedMessage);
        var result = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);
        var cancellationToken = CancellationToken.None;

        webSocket
            .SendAsync(Arg.Any<ArraySegment<byte>>(), WebSocketMessageType.Text, true, cancellationToken)
            .Returns(Task.CompletedTask);

        // When: HandleMessageAsync is called
        await _handler.HandleMessageAsync(webSocket, result, buffer);

        // Then: The message should be logged and sent
        _logger.Received(1).Log(
            LogLevel.Information,
            0,
            Arg.Is<object>(o => o.ToString() == "Received message: Hello, WebSocket!"),
            null,
            Arg.Any<Func<object, Exception, string>>()!);

        await webSocket.Received(1).SendAsync(
            Arg.Is<ArraySegment<byte>>(segment =>
                segment.Array != null && Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count) ==
                receivedMessage),
            WebSocketMessageType.Text,
            true,
            cancellationToken);
    }

    [Test]
    public async Task HandleMessageAsync_WhenExceptionOccurs_ShouldLogError()
    {
        // Given: A WebSocket that throws an exception
        var webSocket = Substitute.For<WebSocket>();
        var buffer = Encoding.UTF8.GetBytes("Test Message");
        var result = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);
        var cancellationToken = CancellationToken.None;

        webSocket
            .SendAsync(Arg.Any<ArraySegment<byte>>(), WebSocketMessageType.Text, true, cancellationToken)
            .Throws(new WebSocketException("Test Exception"));

        // When: HandleMessageAsync is called
        await _handler.HandleMessageAsync(webSocket, result, buffer);

        // Then: An error should be logged
        _logger.Received(1).LogError(Arg.Any<WebSocketException>(), "Error handling WebSocket message");
    }
}