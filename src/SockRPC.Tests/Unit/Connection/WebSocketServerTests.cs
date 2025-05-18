using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SockRPC.Core.Configuration;
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
        _mockWebSocketBufferManager = Substitute.For<IWebSocketBufferManager>();
        _webSocketSettings = new WebSocketSettings();

        _server = new WebSocketServer(
            _mockConnectionAcceptor,
            _mockLogger,
            _mockMessageProcessor,
            _mockWebSocketBufferManager,
            _webSocketSettings
        );
    }

    private IWebSocketConnectionAcceptor _mockConnectionAcceptor;
    private ILogger<WebSocketServer> _mockLogger;
    private IWebSocketMessageProcessor _mockMessageProcessor;
    private IWebSocketBufferManager _mockWebSocketBufferManager;
    private WebSocketSettings _webSocketSettings;
    private WebSocketServer _server;

    [Test]
    public async Task HandleRequest_ValidContext_CallsAcceptConnectionAndStartsProcessingMessages()
    {
        // Given: A valid WebSocket context and all dependencies mocked
        var mockWebSocket = Substitute.For<WebSocket>();
        mockWebSocket.State.Returns(WebSocketState.Open, WebSocketState.Closed);

        var buffer = new byte[1024];
        _mockWebSocketBufferManager.RentBuffer(Arg.Any<int>()).Returns(buffer);

        var context = new DefaultHttpContext();
        var webSocketFeature = Substitute.For<IHttpWebSocketFeature>();
        webSocketFeature.IsWebSocketRequest.Returns(true);
        context.Features.Set(webSocketFeature);

        _mockConnectionAcceptor.AcceptConnectionAsync(context).Returns(mockWebSocket);

        _mockMessageProcessor
            .ProcessMessageAsync(Arg.Any<WebSocket>(), Arg.Any<byte[]>(), Arg.Any<WebSocketSettings>())
            .Returns(Task.CompletedTask);

        // When: HandleRequest is called
        await _server.HandleRequest(context);

        // Then: Connection is accepted, message processing starts, and buffer is returned
        await _mockConnectionAcceptor.Received(1).AcceptConnectionAsync(context);
        await _mockMessageProcessor.Received(1)
            .ProcessMessageAsync(mockWebSocket, buffer, Arg.Any<WebSocketSettings>());
        _mockWebSocketBufferManager.Received(1).ReturnBuffer(buffer);
    }

    [Test]
    public async Task HandleRequest_WhenAcceptConnectionFails_LogsError()
    {
        // Given: Connection acceptor throws an exception
        var context = new DefaultHttpContext();
        var testException = new InvalidOperationException("Test exception");
        _mockConnectionAcceptor.AcceptConnectionAsync(context).Throws(testException);

        // When: HandleRequest is called
        await _server.HandleRequest(context);

        // Then: Error is logged
        _mockLogger.Received(1).LogError(testException, "Error handling WebSocket request");
    }

    [Test]
    public async Task Process_WhenWebSocketExceptionOccurs_CallsHandleDisconnect()
    {
        // Given: A WebSocket that throws an exception during message processing
        var mockWebSocket = Substitute.For<WebSocket>();
        mockWebSocket.State.Returns(WebSocketState.Open); // Will loop once

        var buffer = new byte[1024];
        _mockWebSocketBufferManager.RentBuffer(Arg.Any<int>()).Returns(buffer);

        _mockMessageProcessor
            .ProcessMessageAsync(mockWebSocket, buffer, Arg.Any<WebSocketSettings>())
            .Throws(new WebSocketException("Simulated WebSocket failure"));

        // When: Process is called
        await _server.Process(mockWebSocket);

        // Then: Disconnect is handled and buffer is returned
        await _mockConnectionAcceptor.Received(1).HandleDisconnectAsync(mockWebSocket);
        _mockWebSocketBufferManager.Received(1).ReturnBuffer(buffer);
    }

    [Test]
    public async Task Process_Always_ReturnsBuffer()
    {
        // Given: A WebSocket that opens and closes normally
        var mockWebSocket = Substitute.For<WebSocket>();
        mockWebSocket.State.Returns(WebSocketState.Open, WebSocketState.Closed);

        var buffer = new byte[1024];
        _mockWebSocketBufferManager.RentBuffer(Arg.Any<int>()).Returns(buffer);

        _mockMessageProcessor
            .ProcessMessageAsync(Arg.Any<WebSocket>(), Arg.Any<byte[]>(), Arg.Any<WebSocketSettings>())
            .Returns(Task.CompletedTask);

        // When: Process is called
        await _server.Process(mockWebSocket);

        // Then: Buffer is returned after processing
        _mockWebSocketBufferManager.Received(1).ReturnBuffer(buffer);
    }
}