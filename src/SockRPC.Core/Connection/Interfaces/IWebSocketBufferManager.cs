namespace SockRPC.Core.Connection.Interfaces;

public interface IWebSocketBufferManager
{
    byte[] RentBuffer(int bufferSize);
    void ReturnBuffer(byte[] buffer);
}