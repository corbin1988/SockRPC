using System.Net.WebSockets;
using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using SockRPC.Core.Middleware.Interfaces;
using SockRPC.Core.Routing;

namespace SockRPC.Tests.Unit.Routing;

[TestFixture]
public class RouteRegistryTests
{
    private RouteRegistry _registry;
    private Func<JsonElement, string, WebSocket, Task> _handler;
    private List<IWebSocketMiddleware> _middleware;

    [SetUp]
    public void SetUp()
    {
        _registry = new RouteRegistry();
        _handler = Substitute.For<Func<JsonElement, string, WebSocket, Task>>();
        _middleware = new List<IWebSocketMiddleware> { Substitute.For<IWebSocketMiddleware>() };
    }

    [Test]
    public void HandlerAndMiddleware_Register_RouteIsRegistered()
    {
        // When
        _registry.Register("testMethod", _handler, _middleware);

        // Then
        var route = _registry.GetRoute("testMethod");
        route.Should().NotBeNull();
        route!.Handler.Should().Be(_handler);
        route.WebSocketMiddleware.Should().BeEquivalentTo(_middleware);
    }
    
    [Test]
    public void DuplicateRegistration_Register_PreviousHandlerIsOverwritten()
    {
        // Given
        var newHandler = Substitute.For<Func<JsonElement, string, WebSocket, Task>>();
        _registry.Register("testMethod", _handler);

        // When
        _registry.Register("testMethod", newHandler);

        // Then
        var route = _registry.GetRoute("testMethod");
        route.Should().NotBeNull();
        route!.Handler.Should().Be(newHandler);
    }
    
    [Test]
    public void RegisteredMethod_GetRoute_ReturnsRoute()
    {
        // Given
        _registry.Register("testMethod", _handler);

        // When
        var route = _registry.GetRoute("testMethod");

        // Then
        route.Should().NotBeNull();
        route!.Handler.Should().Be(_handler);
    }
    
    [Test]
    public void UnregisteredMethod_GetRoute_ReturnsNull()
    {
        // When
        var route = _registry.GetRoute("unknownMethod");

        // Then
        route.Should().BeNull();
    }
}