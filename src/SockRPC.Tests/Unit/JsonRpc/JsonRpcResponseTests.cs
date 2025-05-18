using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using SockRPC.Core.JsonRpc;

namespace SockRPC.Tests.Unit.JsonRpc;

[TestFixture]
public class JsonRpcResponseTests
{
    [Test]
    public void Given_Response_When_Serialized_Then_ProducesExpectedJson()
    {
        // Given
        var response = Substitute.For<JsonRpcResponse>("2.0", "1");
        response.Result = JsonDocument.Parse("{}").RootElement;

        // When
        var json = JsonSerializer.Serialize(response);

        // Then
        json.Should().Contain("\"jsonrpc\":\"2.0\"")
            .And.Contain("\"id\":\"1\"");
    }
}