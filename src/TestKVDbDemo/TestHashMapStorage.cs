using KVDbDemo.Storage;

namespace TestKVDbDemo;

[TestClass]
public sealed class TestHashMapStorage: TestBaseStorage
{
    protected override IStorage CreateStorage(int capacity)
    {
        return new HashMapStorage(capacity);
    }
}