namespace KVDbDemo.Storage;

public interface IStorage: IDisposable
{
    unsafe (int Value, bool Success) Retrieve(int key);
    unsafe void Insert(int key, int value);
    unsafe void Remove(int key);
}