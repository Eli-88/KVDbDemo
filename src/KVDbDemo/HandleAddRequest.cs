using System.Text.Json;
using KVDbDemo.Storage;

namespace KVDbDemo;

public class HandleAddRequest: IHandleRequest
{
    public string OnRequest(IStorage storage, string body)
    {
        AddMsg msg = JsonSerializer.Deserialize<AddMsg>(body);
        storage.Insert(msg.Key, msg.Value);
        return "success";
    }
}