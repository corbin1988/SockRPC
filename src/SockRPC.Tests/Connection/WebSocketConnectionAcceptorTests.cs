using System.Net.WebSockets;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NSubstitute;
using SockRPC.Core.Connection;

namespace SockRPC.Tests.Connection;

[TestFixture]
public class WebSocketConnectionAcceptorTests
{
    [SetUp]
    public void SetUp()
    {
        _acceptor = new WebSocketConnectionAcceptor();
    }

    private WebSocketConnectionAcceptor _acceptor;

    private (DefaultHttpContext context, IHttpWebSocketFeature mockFeature, WebSocket mockWebSocket) CreateMockContext(
        bool isWebSocketRequest)
    {
        var context = new DefaultHttpContext();
        var mockFeature = Substitute.For<IHttpWebSocketFeature>();
        var mockWebSocket = Substitute.For<WebSocket>();

        context.Features.Set(mockFeature);
        mockFeature.IsWebSocketRequest.Returns(isWebSocketRequest);
        mockFeature.AcceptAsync(null!).Returns(mockWebSocket);

        return (context, mockFeature, mockWebSocket);
    }

    [Test]
    public async Task AcceptConnectionAsync_WhenValidWebSocketRequest_ReturnsWebSocket()
    {
        // Given
        var (context, _, mockWebSocket) = CreateMockContext(true);

        // When
        var result = await _acceptor.AcceptConnectionAsync(context);

        // Then
        result.Should().Be(mockWebSocket);
    }

    [Test]
    public void AcceptConnectionAsync_WhenInvalidWebSocketRequest_ThrowsException()
    {
        // Given
        var (context, _, _) = CreateMockContext(false);

        // When
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _acceptor.AcceptConnectionAsync(context));

        // Then
        exception.Message.Should().Be("Invalid WebSocket request.");
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task HandleDisconnectAsync_ClosesWebSocketWithNormalClosure()
    {
        // Given
        var mockWebSocket = Substitute.For<WebSocket>();

        // When
        await _acceptor.HandleDisconnectAsync(mockWebSocket);

        // Then
        await mockWebSocket.Received(1).CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Closed by client",
            Arg.Any<CancellationToken>());
    }
}