using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SockRPC.Core.Handling;
using SockRPC.Core.JsonRpc;
using SockRPC.Core.JsonRpc.Interfaces;
using SockRPC.Core.Routing.Interfaces;

namespace SockRPC.Tests.Unit.Handling;

[TestFixture]
public class JsonRpcWebSocketHandlerTests
{
    [SetUp]
    public void SetUp()
    {
        var jsonRpcLogger = Substitute.For<ILogger<JsonRpcWebSocketHandler>>();
        _requestParser = Substitute.For<IJsonRpcRequestParser>();
        _routeExecutor = Substitute.For<IRouteExecutor>();
        _handler = new JsonRpcWebSocketHandler(_requestParser, _routeExecutor, jsonRpcLogger);
        _webSocket = Substitute.For<WebSocket>();
    }

    [TearDown]
    public void TearDown()
    {
        _webSocket.Dispose();
    }

    private IJsonRpcRequestParser _requestParser;
    private JsonRpcWebSocketHandler _handler;
    private WebSocket _webSocket;
    private IRouteExecutor _routeExecutor;

    [Test]
    public async Task ValidRequest_HandleMessageAsync_ParsesIncomingWebSocketMessageCorrectly()
    {
        // Given
        const string validJson = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}";
        var buffer = Encoding.UTF8.GetBytes(validJson);
        var result = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);
        var request = new JsonRpcRequest("2.0", "test", JsonDocument.Parse("{}").RootElement, "1");

        _requestParser.ParseAndValidate(validJson).Returns(request);

        // When
        await _handler.HandleMessageAsync(_webSocket, result, buffer);

        // Then
        _requestParser.Received(1).ParseAndValidate(validJson);
    }

    [Test]
    public async Task InvalidJsonRpcRequest_HandleMessageAsync_SendsValidationErrorResponse()
    {
        // Given
        const string invalidJson = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}";
        var buffer = Encoding.UTF8.GetBytes(invalidJson);
        var result = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);
        var errorResponse = new JsonRpcResponse("2.0", "1")
        {
            Error = new JsonRpcError("Validation error", null) { Code = -32602 }
        };

        _requestParser
            .When(x => x.ParseAndValidate(invalidJson))
            .Do(_ => throw new JsonRpcValidationException(errorResponse));

        // When
        await _handler.HandleMessageAsync(_webSocket, result, buffer);

        // Then
        await _webSocket.Received(1).SendAsync(
            Arg.Is<ArraySegment<byte>>(segment =>
                segment.Array != null && Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count)
                    .Contains("\"code\":-32602")),
            WebSocketMessageType.Text,
            true,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ValidRequest_HandleMessageAsync_DelegatesToRouteExecutor()
    {
        // Given
        const string validJson = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}";
        var buffer = Encoding.UTF8.GetBytes(validJson);
        var result = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);
        var request = new JsonRpcRequest("2.0", "test", JsonDocument.Parse("{}").RootElement, "1");
        var context = new JsonRpcContext(request, _webSocket);

        _requestParser.ParseAndValidate(validJson).Returns(request);

        // When
        await _handler.HandleMessageAsync(_webSocket, result, buffer);

        // Then
        await _routeExecutor.Received(1).ExecuteAsync(Arg.Is<JsonRpcContext>(ctx =>
            ctx.Request.Method == "test" && ctx.Request.Id == "1"));
    }

    [Test]
    public async Task Exception_HandleMessageAsync_SendsInternalErrorResponse()
    {
        // Given
        const string validJson = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}";
        var buffer = Encoding.UTF8.GetBytes(validJson);
        var result = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);

        _requestParser
            .When(x => x.ParseAndValidate(validJson))
            .Do(_ => throw new Exception("Unexpected error"));

        // When
        await _handler.HandleMessageAsync(_webSocket, result, buffer);

        // Then
        await _webSocket.Received(1).SendAsync(
            Arg.Is<ArraySegment<byte>>(segment =>
                segment.Array != null && Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count)
                    .Contains("\"code\":-32603")),
            WebSocketMessageType.Text,
            true,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ValidRequest_HandleMessageAsync_ParsesAndValidatesRequest()
    {
        // Given
        const string validJson = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}";
        var buffer = Encoding.UTF8.GetBytes(validJson);
        var result = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);
        var request = new JsonRpcRequest("2.0", "test", JsonDocument.Parse("{}").RootElement, "1");

        _requestParser.ParseAndValidate(validJson).Returns(request);

        // When
        await _handler.HandleMessageAsync(_webSocket, result, buffer);

        // Then
        _requestParser.Received(1).ParseAndValidate(validJson);
    }

    [Test]
    public async Task InvalidRequest_HandleMessageAsync_SendsValidationErrorResponse()
    {
        // Given
        var invalidJson = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}";
        var buffer = Encoding.UTF8.GetBytes(invalidJson);
        var result = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);
        var errorResponse = new JsonRpcResponse("2.0", "1")
        {
            Error = new JsonRpcError("Validation error", null) { Code = -32602 }
        };

        _requestParser
            .When(x => x.ParseAndValidate(invalidJson))
            .Do(_ => throw new JsonRpcValidationException(errorResponse));

        // When
        await _handler.HandleMessageAsync(_webSocket, result, buffer);

        // Then
        await _webSocket.Received(1).SendAsync(
            Arg.Is<ArraySegment<byte>>(segment =>
                segment.Array != null && Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count)
                    .Contains("\"code\":-32602")),
            WebSocketMessageType.Text,
            true,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task InvalidJson_HandleMessageAsync_SendsParseErrorResponse()
    {
        // Given
        const string invalidJson = "{invalid}";
        var buffer = Encoding.UTF8.GetBytes(invalidJson);
        var result = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);

        _requestParser
            .When(x => x.ParseAndValidate(Arg.Any<string>()))
            .Do(_ => throw new JsonException());

        // When
        await _handler.HandleMessageAsync(_webSocket, result, buffer);

        // Then
        await _webSocket.Received(1).SendAsync(
            Arg.Is<ArraySegment<byte>>(segment =>
                segment.Array != null && Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count)
                    .Contains("\"code\":-32700")),
            WebSocketMessageType.Text,
            true,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task UnexpectedException_HandleMessageAsync_SendsInternalErrorResponse()
    {
        // Given
        const string validJson = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}";
        var buffer = Encoding.UTF8.GetBytes(validJson);
        var result = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true);

        _requestParser
            .When(x => x.ParseAndValidate(validJson))
            .Do(_ => throw new Exception("Unexpected error"));

        // When
        await _handler.HandleMessageAsync(_webSocket, result, buffer);

        // Then
        await _webSocket.Received(1).SendAsync(
            Arg.Is<ArraySegment<byte>>(segment =>
                segment.Array != null && Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count)
                    .Contains("\"code\":-32603")),
            WebSocketMessageType.Text,
            true,
            Arg.Any<CancellationToken>());
    }
}