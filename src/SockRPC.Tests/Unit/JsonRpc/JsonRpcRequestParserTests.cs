using FluentAssertions;
using NSubstitute;
using SockRPC.Core.JsonRpc;
using SockRPC.Core.JsonRpc.Interfaces;

namespace SockRPC.Tests.Unit.JsonRpc;

[TestFixture]
public class JsonRpcRequestParserTests
{
    [SetUp]
    public void SetUp()
    {
        _validator = Substitute.For<IJsonRpcValidator>();
        _parser = new JsonRpcRequestParser(_validator);
    }

    private IJsonRpcValidator _validator;
    private JsonRpcRequestParser _parser;

    [Test]
    public void Given_ValidRequest_When_ParseAndValidate_Then_ReturnsRequest()
    {
        // Given
        var validJson = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}";
        _validator.ValidateRequest(Arg.Any<JsonRpcRequest>()).Returns((JsonRpcResponse)null!);

        // When
        var result = _parser.ParseAndValidate(validJson);

        // Then
        result.Should().NotBeNull();
        result.Jsonrpc.Should().Be("2.0");
        result.Method.Should().Be("test");
        result.Id.Should().Be("1");
    }

    [Test]
    public void Given_InvalidRequest_When_ParseAndValidate_Then_ThrowsValidationException()
    {
        // Given
        var validJson = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}";
        var errorResponse = Substitute.For<JsonRpcResponse>("2.0", "1");
        _validator.ValidateRequest(Arg.Any<JsonRpcRequest>()).Returns(errorResponse);

        // When
        var act = () => _parser.ParseAndValidate(validJson);

        // Then
        act.Should().Throw<JsonRpcValidationException>()
            .Which.ErrorResponse.Should().Be(errorResponse);
    }

    [Test]
    public void Given_InvalidJson_When_Parse_Then_ThrowsParseException()
    {
        // Given
        var invalidJson = "{invalid}";

        // When
        var act = () => _parser.Parse(invalidJson);

        // Then
        act.Should().Throw<JsonRpcException>()
            .WithMessage("Parse error: Invalid JSON.");
    }
}