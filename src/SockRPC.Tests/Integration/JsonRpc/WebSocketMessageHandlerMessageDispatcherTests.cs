using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SockRPC.Core.Handling;
using SockRPC.Core.JsonRpc;
using SockRPC.Core.JsonRpc.Interfaces;

namespace SockRPC.Tests.Integration.JsonRpc;

[TestFixture]
public class WebSocketMessageHandlerMessageDispatcherTests : WebSocketIntegrationTestsBase
{
    [Test]
    public async Task Given_ValidJsonRpcRequest_When_Processed_Then_ShouldReturnExpectedResponse()
    {
        // Given
        var id = Guid.NewGuid().ToString();
        var validRequest = $"{{\"jsonrpc\":\"2.0\",\"method\":\"test.method\",\"params\":{{}},\"id\":\"{id}\"}}";
        var expectedAcknowledgment = new
        {
            jsonrpc = "2.0",
            result = "Acknowledged",
            id = id
        };
        var buffer = Encoding.UTF8.GetBytes(validRequest);

        // When
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await ClientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cts.Token);

            var responseBuffer = new byte[16384];
            var result = await ClientWebSocket.ReceiveAsync(new ArraySegment<byte>(responseBuffer), cts.Token);
            var responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, result.Count);

            Console.WriteLine($"Actual Response: {responseMessage}");

            // Deserialize the actual response into an anonymous object
            var actualResponse = JsonSerializer.Deserialize(responseMessage, expectedAcknowledgment.GetType());

            // Then
            actualResponse.Should().BeEquivalentTo(expectedAcknowledgment);
        }
        catch (TaskCanceledException)
        {
            Assert.Fail("The operation was canceled due to a timeout or connection issue.");
        }
        catch (WebSocketException ex)
        {
            Assert.Fail($"WebSocket error: {ex.Message}");
        }
    }

    [Test]
    public async Task Given_RawMessage_When_Dispatched_Then_RawHandlerProcessesMessage()
    {
        // Given
        const string rawMessage = "RAW MESSAGE";
        var buffer = Encoding.UTF8.GetBytes(rawMessage);

        // When
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await ClientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cts.Token);

        var responseBuffer = new byte[16384];
        var result = await ClientWebSocket.ReceiveAsync(new ArraySegment<byte>(responseBuffer), cts.Token);
        var responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, result.Count);

        // Then
        responseMessage.Should().Contain(rawMessage);
    }
}