using System.Net.WebSockets;
using System.Text;
using FluentAssertions;
using NSubstitute;
using SockRPC.Core.Connection;

namespace SockRPC.Tests.Connection;

[TestFixture]
public class WebSocketMessageProcessorTests
{
    [SetUp]
    public void SetUp()
    {
        // Given: A WebSocketMessageProcessor and a mock WebSocket
        _messageProcessor = new WebSocketMessageProcessor();
        _mockWebSocket = Substitute.For<WebSocket>();
    }

    [TearDown]
    public void TearDown()
    {
        _mockWebSocket?.Dispose();
    }

    private WebSocketMessageProcessor _messageProcessor;
    private WebSocket _mockWebSocket;

    [Test]
    public async Task ProcessMessageAsync_ShouldThrowException_WhenMessageTypeIsClose()
    {
        // Given: A WebSocket that returns a Close message type
        var buffer = new byte[1024];
        _mockWebSocket
            .ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(
                new WebSocketReceiveResult(0, WebSocketMessageType.Close, true)));

        // When: ProcessMessageAsync is called
        var act = async () => await _messageProcessor.ProcessMessageAsync(_mockWebSocket, buffer);

        // Then: A WebSocketException should be thrown
        await act.Should().ThrowAsync<WebSocketException>()
            .WithMessage("WebSocket connection closed.");
    }

    [Test]
    public async Task ProcessMessageAsync_ShouldProcessTextMessage_WhenMessageTypeIsText()
    {
        // Given: A WebSocket that returns a Text message type
        var buffer = new byte[1024];
        var message = "Hello, WebSocket!";
        var messageBytes = Encoding.UTF8.GetBytes(message);

        _mockWebSocket
            .ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(
                    new WebSocketReceiveResult(messageBytes.Length, WebSocketMessageType.Text, true)))
            .AndDoes(call =>
            {
                var segment = call.Arg<ArraySegment<byte>>();
                messageBytes.CopyTo(segment.Array, segment.Offset);
            });

        // When: ProcessMessageAsync is called
        await _messageProcessor.ProcessMessageAsync(_mockWebSocket, buffer);

        // Then: The buffer should contain the received message
        var receivedMessage = Encoding.UTF8.GetString(buffer, 0, messageBytes.Length);
        receivedMessage.Should().Be(message);
    }
}