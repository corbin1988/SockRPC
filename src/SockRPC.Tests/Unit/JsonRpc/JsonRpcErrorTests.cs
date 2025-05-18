using FluentAssertions;
using NSubstitute;
using SockRPC.Core.JsonRpc;

namespace SockRPC.Tests.Unit.JsonRpc;

[TestFixture]
public class JsonRpcErrorTests
{
    [Test]
    public void Given_Error_When_Constructed_Then_PropertiesAreSet()
    {
        // Given
        const string message = "Error occurred";
        const string data = "Error details";

        // When
        var error = Substitute.For<JsonRpcError>(message, data);

        // Then
        error.Message.Should().Be(message);
        error.Data.Should().Be(data);
    }
}