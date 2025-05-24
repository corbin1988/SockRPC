using System.Net.WebSockets;
using System.Text;
using FluentAssertions;
using NSubstitute;
using SockRPC.Core.Configuration;
using SockRPC.Core.Connection;
using SockRPC.Core.Handling.Interfaces;

namespace SockRPC.Tests.Connection;

[TestFixture]
public class WebSocketMessageProcessorTests
{
    [SetUp]
    public void SetUp()
    {
        // IWebSocketHandlerDispatcher
        _mockWebSocketHandlerMessageDispatcher = Substitute.For<IWebSocketHandlerMessageDispatcher>();
        _messageProcessor = new WebSocketMessageProcessor(_mockWebSocketHandlerMessageDispatcher);
        _mockWebSocket = Substitute.For<WebSocket>();
        _webSocketSettings = new WebSocketSettings
        {
            BufferSize = 1024,
            MaxMessageSize = 2048
        };
    }

    [TearDown]
    public void TearDown()
    {
        _mockWebSocket?.Dispose();
    }

    private WebSocketMessageProcessor _messageProcessor;
    private WebSocket _mockWebSocket;
    private WebSocketSettings _webSocketSettings;
    private IWebSocketHandlerMessageDispatcher _mockWebSocketHandlerMessageDispatcher;

    [Test]
    public async Task ReadFullMessageAsync_ShouldReturnMessage_WhenMessageIsReceived()
    {
        // Given: A WebSocket that returns a text message within size limits
        var buffer = new byte[_webSocketSettings.BufferSize];
        var message = "Hello, WebSocket!";
        var messageBytes = Encoding.UTF8.GetBytes(message);

        _mockWebSocket
            .ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var segment = call.Arg<ArraySegment<byte>>();
                messageBytes.CopyTo(segment.Array!, segment.Offset);
                return Task.FromResult(new WebSocketReceiveResult(messageBytes.Length, WebSocketMessageType.Text,
                    true));
            });

        // When: ReadFullMessageAsync is called
        var result = await _messageProcessor.ReadFullMessageAsync(
            _mockWebSocket,
            buffer,
            _webSocketSettings,
            CancellationToken.None
        );

        // Then: The returned message should match the original
        result.Should().Be(message);
    }

    [Test]
    public async Task ReadFullMessageAsync_ShouldThrowException_WhenMessageTypeIsClose()
    {
        // Given: A WebSocket that immediately sends a Close message
        var buffer = new byte[_webSocketSettings.BufferSize];

        _mockWebSocket
            .ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(
                new WebSocketReceiveResult(0, WebSocketMessageType.Close, true)));

        // When: ReadFullMessageAsync is called
        var act = async () =>
            await _messageProcessor.ReadFullMessageAsync(
                _mockWebSocket,
                buffer,
                _webSocketSettings,
                CancellationToken.None
            );

        // Then: A WebSocketException should be thrown
        await act.Should().ThrowAsync<WebSocketException>()
            .WithMessage("WebSocket connection closed.");
    }

    [Test]
    public async Task ReadFullMessageAsync_ShouldThrowException_WhenMessageExceedsMaxSize()
    {
        // Given: A WebSocket that sends a message larger than MaxMessageSize
        var buffer = new byte[_webSocketSettings.BufferSize];
        var largeMessage = new byte[_webSocketSettings.MaxMessageSize + 1];
        var currentOffset = 0;

        _mockWebSocket
            .ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var segment = call.Arg<ArraySegment<byte>>();
                var bytesToCopy = Math.Min(segment.Count, largeMessage.Length - currentOffset);

                if (bytesToCopy <= 0)
                    return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Text, true));

                Array.Copy(largeMessage, currentOffset, segment.Array!, segment.Offset, bytesToCopy);
                currentOffset += bytesToCopy;

                return Task.FromResult(new WebSocketReceiveResult(bytesToCopy, WebSocketMessageType.Text,
                    currentOffset >= largeMessage.Length));
            });

        // When: ReadFullMessageAsync is called
        var act = async () =>
            await _messageProcessor.ReadFullMessageAsync(
                _mockWebSocket,
                buffer,
                _webSocketSettings,
                CancellationToken.None
            );

        // Then: An InvalidOperationException should be thrown
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("WebSocket message exceeded allowed size.");
    }
}