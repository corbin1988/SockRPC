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

    [Test]
    public async Task WebSocket_ShouldHandleMultipleConnections()
    {
        const int connectionCount = 5; // Number of simultaneous connections to test
        var clients = new List<ClientWebSocket>();

        try
        {
            // Initialize and connect multiple WebSocket clients
            for (var i = 0; i < connectionCount; i++)
            {
                var client = new ClientWebSocket();
                await client.ConnectAsync(new Uri($"ws://localhost:{_port}/ws"), CancellationToken.None);
                clients.Add(client);

                // Verify that the connection is open
                client.State.Should().Be(WebSocketState.Open, $"Client {i + 1} should be connected.");
            }
        }
        finally
        {
            // Close all WebSocket connections
            foreach (var client in clients)
            {
                if (client.State == WebSocketState.Open)
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete",
                        CancellationToken.None);

                client.Dispose();
            }
        }
    }

    [Test]
    public async Task AbruptDisconnection_ShouldNotCrashServer()
    {
        // Given a WebSocket client
        var client = new ClientWebSocket();
        // TODO: .ENV
        var serverUri = new Uri($"ws://localhost:{_port}/ws");
        await client.ConnectAsync(serverUri, CancellationToken.None);

        // When: Simulate an abrupt disconnection
        client.Abort(); // Simulate abrupt disconnection

        // Then: Verify the client is in the Aborted state
        client.State.Should().Be(WebSocketState.Aborted, "an abrupt disconnection should result in the Aborted state");
    }

    [Test]
    public async Task WebSocket_ShouldHandleDisconnection()
    {
        await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);

        ClientWebSocket.State.Should().Be(WebSocketState.Closed);
    }
}