using System.Text.Json;
using KVDbDemo.Storage;

namespace KVDbDemo;

public class HandleRemoveRequest: IHandleRequest
{
    public string OnRequest(IStorage storage, string body)
    {
        RemoveMsg msg = JsonSerializer.Deserialize<RemoveMsg>(body);
        storage.Remove(msg.Key);
        return "success";
    }
}