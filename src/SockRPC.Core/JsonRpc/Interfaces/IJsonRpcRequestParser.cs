namespace SockRPC.Core.JsonRpc.Interfaces;

public interface IJsonRpcRequestParser
{
    JsonRpcRequest ParseAndValidate(string message);
}