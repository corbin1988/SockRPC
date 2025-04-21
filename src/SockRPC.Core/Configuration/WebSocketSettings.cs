namespace SockRPC.Core.Configuration;

public class WebSocketSettings
{
    public int BufferSize { get; set; } = 1024 * 4;
    public int MaxMessageSize { get; set; } = 1024 * 1024 * 10;
}