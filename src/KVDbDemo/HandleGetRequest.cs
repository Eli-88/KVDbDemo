using System.Text.Json;
using KVDbDemo.Storage;

namespace KVDbDemo;

public class HandleGetRequest: IHandleRequest
{
    public string OnRequest(IStorage storage, string body)
    {
        GetMsg msg = JsonSerializer.Deserialize<GetMsg>(body);
        (int value, bool success) = storage.Retrieve(msg.Key);
        return success ? $"{value}" : "key not found";
    }
}