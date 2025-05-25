using System.Text.Json;
using FluentAssertions;
using SockRPC.Core.JsonRpc;

namespace SockRPC.Tests.Unit.JsonRpc;

[TestFixture]
public class JsonRpcValidatorTests
{
    [SetUp]
    public void SetUp()
    {
        _validator = new JsonRpcValidator();
    }

    private JsonRpcValidator _validator;

    [Test]
    public void ValidateRequest_ShouldReturnErrorResponse_WhenJsonRpcVersionIsInvalid()
    {
        // Given
        var request = new JsonRpcRequest("1.0", "test.method", JsonDocument.Parse("{}").RootElement, "1");

        // When
        var response = _validator.ValidateRequest(request);

        // Then
        response.Should().NotBeNull();
        response.Error.Should().NotBeNull();
        response.Error!.Code.Should().Be(-32600);
        response.Error.Message.Should().Be("Invalid JSON-RPC version.");
    }

    [Test]
    public void ValidateRequest_ShouldReturnErrorResponse_WhenMethodFormatIsInvalid()
    {
        // Given
        var request = new JsonRpcRequest("2.0", "invalidMethod", JsonDocument.Parse("{}").RootElement, "1");

        // When
        var response = _validator.ValidateRequest(request);

        // Then
        response.Should().NotBeNull();
        response.Error.Should().NotBeNull();
        response.Error!.Code.Should().Be(-32601);
        response.Error.Message.Should().Be("Method not found: Expected 'topic.action' format.");
    }

    [Test]
    public void ValidateRequest_ShouldReturnErrorResponse_WhenParamsAreNull()
    {
        // Given
        var request = new JsonRpcRequest("2.0", "test.method", JsonDocument.Parse("null").RootElement, "1");

        // When
        var response = _validator.ValidateRequest(request);

        // Then
        response.Should().NotBeNull();
        response.Error.Should().NotBeNull();
        response.Error!.Code.Should().Be(-32602);
        response.Error.Message.Should().Be("Invalid params: Parameters cannot be null.");
    }

    [Test]
    public void ValidateRequest_ShouldReturnNull_WhenRequestIsValid()
    {
        // Given
        var request = new JsonRpcRequest("2.0", "test.method", JsonDocument.Parse("{}").RootElement, "1");

        // When
        var response = _validator.ValidateRequest(request);

        // Then
        response.Should().BeNull();
    }

    [Test]
    public void CreateErrorResponse_ShouldReturnCorrectErrorResponse_ForMethodNotFound()
    {
        // Given
        const string method = "unknownMethod";
        const string id = "1";

        // When
        var response = _validator.MethodNotFound(method, id);

        // Then
        response.Should().NotBeNull();
        response.Error.Should().NotBeNull();
        response.Error!.Code.Should().Be(-32601);
        response.Error.Message.Should().Be($"Method not found: {method}");
    }

    [Test]
    public void CreateErrorResponse_ShouldReturnCorrectErrorResponse_ForInternalError()
    {
        // Given
        const string id = "1";
        const string message = "Internal error occurred";

        // When
        var response = _validator.CreateErrorResponse(-32603, message, id);

        // Then
        response.Should().NotBeNull();
        response.Error.Should().NotBeNull();
        response.Error!.Code.Should().Be(-32603);
        response.Error.Message.Should().Be(message);
    }
}