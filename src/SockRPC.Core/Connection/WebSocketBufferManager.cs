using System.Buffers;
using SockRPC.Core.Connection.Interfaces;

namespace SockRPC.Core.Connection;

public class WebSocketBufferManager : IWebSocketBufferManager
{
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Shared;

    public byte[] RentBuffer(int bufferSize)
    {
        return BufferPool.Rent(bufferSize);
    }

    public void ReturnBuffer(byte[] buffer)
    {
        BufferPool.Return(buffer, true);
    }
}