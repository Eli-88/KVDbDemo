using KVDbDemo.Storage;

namespace KVDbDemo;

public interface IRequestDispatcher
{
    string? OnDispatch(IStorage storage, string path, string body);
}