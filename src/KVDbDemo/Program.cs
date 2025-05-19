using KVDbDemo.Storage;

namespace KVDbDemo;

internal static class Program
{
    public static void Main()
    {
        using IStorage storage = new SkipListStorage(1 << 20);
        
        Service service = new Service("localhost", 8080);
        service.MapRequest("/add", new HandleAddRequest());
        service.MapRequest("/remove", new HandleRemoveRequest());
        service.MapRequest("/get", new HandleGetRequest());
        
        service.Run(storage);
    }
}