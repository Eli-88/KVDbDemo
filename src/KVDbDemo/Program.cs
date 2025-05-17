using KVDbDemo.Storage;

namespace KVDbDemo;

internal static class Program
{
    public static void Main()
    {
        using IStorage storage = new SkipListStorage(1 << 20);
        
        RequestDispatcher requestDispatcher = new();
        requestDispatcher.Add("/add", new HandleAddRequest());
        requestDispatcher.Add("/remove", new HandleRemoveRequest());
        requestDispatcher.Add("/get", new HandleGetRequest());

        Service service = new Service("localhost", 8080, requestDispatcher);
        service.Run(storage);
    }
}