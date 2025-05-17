using KVDbDemo.Storage;

namespace KVDbDemo;

public interface IHandleRequest
{
    string OnRequest(IStorage storage, string body);
}