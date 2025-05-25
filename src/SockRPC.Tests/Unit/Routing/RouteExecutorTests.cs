using System.Net.WebSockets;
using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using SockRPC.Core.JsonRpc;
using SockRPC.Core.JsonRpc.Interfaces;
using SockRPC.Core.Middleware.Interfaces;
using SockRPC.Core.Routing;
using SockRPC.Core.Routing.Interfaces;

[TestFixture]
public class RouteExecutorTests
{
    private RouteRegistry _registry;
    private IWebSocketMiddleware _middleware1;
    private IWebSocketMiddleware _middleware2;
    private Func<JsonElement, string, WebSocket, Task> _handler;
    private IRouteRegistry _mockRegistry;
    private IJsonRpcValidator _validator;
    private RouteExecutor _executor;

    [SetUp]
    public void SetUp()
    {
        _registry = new RouteRegistry();
        _middleware1 = Substitute.For<IWebSocketMiddleware>();
        _middleware2 = Substitute.For<IWebSocketMiddleware>();
        _handler = Substitute.For<Func<JsonElement, string, WebSocket, Task>>();
        _mockRegistry = Substitute.For<IRouteRegistry>();
        _validator = Substitute.For<IJsonRpcValidator>();
        _executor = new RouteExecutor(_mockRegistry, _validator);
    }

    [Test]
    public void RegisteredMethod_GetRoute_ReturnsCorrectRoute()
    {
        // Given
        _registry.Register("testMethod", _handler);

        // When
        var route = _registry.GetRoute("testMethod");

        // Then
        route.Should().NotBeNull();
        route.Handler.Should().Be(_handler);
    }

    [Test]
    public void UnregisteredMethod_GetRoute_ReturnsNull()
    {
        // When
        var route = _registry.GetRoute("unknownMethod");

        // Then
        route.Should().BeNull();
    }

    [Test]
    public async Task MiddlewareChain_ExecuteAsync_ExecutesInCorrectOrder()
    {
        // Given
        var context = new JsonRpcContext(new JsonRpcRequest("2.0", "testMethod", default, "1"),
            Substitute.For<WebSocket>());
        var routeInfo = new RouteInfo
        {
            Handler = _handler,
            WebSocketMiddleware = new List<IWebSocketMiddleware> { _middleware1, _middleware2 }
        };

        _mockRegistry.GetRoute("testMethod").Returns(routeInfo);

        _middleware1
            .Handle(Arg.Any<JsonRpcContext>(), Arg.Any<Func<Task>>())
            .Returns(async call =>
            {
                var next = call.ArgAt<Func<Task>>(1);
                await next();
            });

        _middleware2
            .Handle(Arg.Any<JsonRpcContext>(), Arg.Any<Func<Task>>())
            .Returns(async call =>
            {
                var next = call.ArgAt<Func<Task>>(1);
                await next();
            });

        // When
        await _executor.ExecuteAsync(context);

        // Then
        Received.InOrder(async void () =>
        {
            await _middleware1.Handle(context, Arg.Any<Func<Task>>());
            await _middleware2.Handle(context, Arg.Any<Func<Task>>());
            await _handler(context.Request.Params, context.Request.Id, context.WebSocket);
        });
    }
}