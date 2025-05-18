using FluentAssertions;
using SockRPC.Core.JsonRpc;

namespace SockRPC.Tests.Unit.JsonRpc;

[TestFixture]
public class JsonRpcExceptionTests
{
    [Test]
    public void Given_Exception_When_Constructed_Then_PropertiesAreSet()
    {
        // Given
        var code = -32700;
        var message = "Parse error";

        // When
        var exception = new JsonRpcException(code, message);

        // Then
        exception.Code.Should().Be(code);
        exception.Message.Should().Be(message);
    }
}