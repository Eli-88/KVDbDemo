using KVDbDemo;
using KVDbDemo.Storage;

namespace TestKVDbDemo;

[TestClass]
public sealed class TestArrayStorage: TestBaseStorage
{
    protected override IStorage CreateStorage(int capacity)
    {
        return new ArrayStorage(capacity);
    }
}