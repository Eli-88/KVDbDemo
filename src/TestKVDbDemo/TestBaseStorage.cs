using KVDbDemo;
using KVDbDemo.Storage;

namespace TestKVDbDemo;

public abstract class TestBaseStorage
{
    protected abstract IStorage CreateStorage(int capacity);
    
    [TestMethod]
    public void TestExceedCapacity()
    {
        using IStorage storage = CreateStorage(10);
        for(int i = 0; i < 10; ++i) {storage.Insert(i, i);}
        Assert.ThrowsException<OutOfMemoryException>(() => storage.Insert(100, 100));
    }

    [TestMethod]
    public void TestJustWithinCapacity()
    {
        try
        {
            using IStorage storage = CreateStorage(10);
            for(int i = 0; i < 10; ++i) {storage.Insert(i, i);}
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [TestMethod]
    public void TestJustWithinCapacityWithRemoval()
    {
        try
        {
            using IStorage storage = CreateStorage(10);
            for(int i = 0; i < 10; ++i) {storage.Insert(i, i);}
            storage.Remove(0);
            storage.Insert(0, 1000);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [TestMethod]
    public void TestInsertAndRetrieve()
    {
        using IStorage storage = CreateStorage(10);
        for(int i = 0; i < 10; ++i) {storage.Insert(i, i);}

        for (int i = 0; i < 10; ++i)
        {
            (int value, bool success) = storage.Retrieve(i);
            Assert.IsTrue(success);
            Assert.AreEqual(i, value);
        }
    }

    [TestMethod]
    public void TestRetrieveEmpty()
    {
        using IStorage storage = CreateStorage(10);
        (_, bool success) = storage.Retrieve(0);
        Assert.IsFalse(success);
    }

    [TestMethod]
    public void TestInsertDuplicateKey()
    {
        using IStorage storage = CreateStorage(10);
        for(int i = 0; i < 10; ++i) {storage.Insert(i, i);}
        for(int i = 0; i < 10; ++i) {storage.Insert(i, i + 100);}

        for (int i = 0; i < 10; ++i)
        {
            (int value, bool success) = storage.Retrieve(i);
            Assert.IsTrue(success);
            Assert.AreEqual(i + 100, value);
        }
    }

    [TestMethod]
    public void TestRemoveNonExistingKey()
    {
        try
        {
            using IStorage storage = CreateStorage(10);
            storage.Remove(0);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}