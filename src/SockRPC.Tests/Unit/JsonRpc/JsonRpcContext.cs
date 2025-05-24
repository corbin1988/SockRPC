using System.Net.WebSockets;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SockRPC.Core.JsonRpc;

namespace SockRPC.Tests.Unit.JsonRpc;

[TestFixture]
public class JsonRpcContextTests
{
    [Test]
    public void Given_ValidParameters_When_Constructed_Then_PropertiesAreSet()
    {
        // Given
        var request = new JsonRpcRequest("2.0", "testMethod", default, "1");
        var webSocket = new ClientWebSocket();
        var httpContext = new DefaultHttpContext();

        // When
        var context = new JsonRpcContext(request, webSocket, httpContext);

        // Then
        context.Request.Should().Be(request);
        context.WebSocket.Should().Be(webSocket);
        context.HttpContext.Should().Be(httpContext);
    }

    [Test]
    public void Given_NullHttpContext_When_Constructed_Then_HttpContextIsNull()
    {
        // Given
        var request = new JsonRpcRequest("2.0", "testMethod", default, "1");
        var webSocket = new ClientWebSocket();

        // When
        var context = new JsonRpcContext(request, webSocket);

        // Then
        context.Request.Should().Be(request);
        context.WebSocket.Should().Be(webSocket);
        context.HttpContext.Should().BeNull();
    }
}