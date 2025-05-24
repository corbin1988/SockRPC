using FluentAssertions;
using NSubstitute;
using SockRPC.Core.JsonRpc;

namespace SockRPC.Tests.Unit.JsonRpc;

[TestFixture]
public class JsonRpcValidationExceptionTests
{
    [Test]
    public void Given_Exception_When_Constructed_Then_ErrorResponseIsSet()
    {
        // Given
        var errorResponse = Substitute.For<JsonRpcResponse>("2.0", "1");

        // When
        var exception = new JsonRpcValidationException(errorResponse);

        // Then
        exception.ErrorResponse.Should().Be(errorResponse);
    }
}