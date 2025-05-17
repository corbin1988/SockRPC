using System.Net.WebSockets;
using FluentAssertions;

namespace SockRPC.Tests.Integration.Connection;

[TestFixture]
public class WebSocketConnectionTests : WebSocketIntegrationTestsBase
{
    [Test]
    public async Task WebSocket_ShouldConnectSuccessfully()
    {
        ClientWebSocket.State.Should().Be(WebSocketState.Open);
    }
}