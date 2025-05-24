namespace SockRPC.Core.JsonRpc;

public class JsonRpcException(int code, string message) : Exception(message)
{
    public int Code { get; } = code;
}