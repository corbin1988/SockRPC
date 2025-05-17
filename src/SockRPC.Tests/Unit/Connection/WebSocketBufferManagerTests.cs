using FluentAssertions;
using SockRPC.Core.Connection;

namespace SockRPC.Tests.Connection;

[TestFixture]
public class WebSocketBufferManagerTests
{
    [SetUp]
    public void SetUp()
    {
        // Given: Create an instance of WebSocketBufferManager
        _bufferManager = new WebSocketBufferManager();
    }

    private WebSocketBufferManager _bufferManager;

    [Test]
    public void RentBuffer_ShouldReturnBufferOfRequestedSize()
    {
        // Given
        var bufferSize = 1024;

        // When: Rent a buffer
        var buffer = _bufferManager.RentBuffer(bufferSize);

        // Then: Assert the buffer is not null and has at least the requested size
        buffer.Should().NotBeNull();
        buffer.Length.Should().BeGreaterThanOrEqualTo(bufferSize);

        // Cleanup: Return the buffer to the pool
        _bufferManager.ReturnBuffer(buffer);
    }

    [Test]
    public void ReturnBuffer_ShouldNotThrowException()
    {
        // Given: Rent a buffer
        var buffer = _bufferManager.RentBuffer(1024);

        // When: Return the buffer
        var act = () => _bufferManager.ReturnBuffer(buffer);

        // Then: Assert no exception is thrown
        act.Should().NotThrow();
    }
}