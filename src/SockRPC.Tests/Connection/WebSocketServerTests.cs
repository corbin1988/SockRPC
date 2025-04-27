using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SockRPC.Core.Connection;
using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Tests.Connection;

[TestFixture]
public class WebSocketServerTests
{
    [SetUp]
    public void SetUp()
    {
        _mockConnectionAcceptor = Substitute.For<IWebSocketConnectionAcceptor>();
        _mockLogger = Substitute.For<ILogger<WebSocketServer>>();
        _mockMessageProcessor = Substitute.For<IWebSocketMessageProcessor>();
        _server = new WebSocketServer(_mockConnectionAcceptor, _mockLogger, _mockMessageProcessor);
    }

    private IWebSocketConnectionAcceptor _mockConnectionAcceptor;
    private ILogger<WebSocketServer> _mockLogger;
    private IWebSocketMessageProcessor _mockMessageProcessor;
    private WebSocketServer _server;

    [Test]
    public async Task HandleRequest_ValidContext_CallsAcceptConnectionAsync()
    {
        // Given
        var mockWebSocket = Substitute.For<WebSocket>();

        var context = new DefaultHttpContext();
        context.Features.Set(Substitute.For<IHttpWebSocketFeature>());
        _mockConnectionAcceptor.AcceptConnectionAsync(context).Returns(mockWebSocket);

        // When
        await _server.HandleRequest(context);

        // Then
        await _mockConnectionAcceptor.Received(1).AcceptConnectionAsync(context);
        await _mockMessageProcessor.Received(1).ProcessMessageAsync(mockWebSocket, Arg.Any<byte[]>());
    }

    [Test]
    public async Task HandleRequest_ExceptionThrown_LogsError()
    {
        // Given
        var context = new DefaultHttpContext();
        var testException = new InvalidOperationException("Test exception");
        _mockConnectionAcceptor.AcceptConnectionAsync(context).Throws(testException);

        // When
        await _server.HandleRequest(context);

        // Then
        _mockLogger.Received(1).LogError(testException, "Error handling WebSocket request");
    }
}