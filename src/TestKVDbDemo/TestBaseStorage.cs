using KVDbDemo;

namespace TestKVDbDemo;

public abstract class TestBaseStorage
{
    protected abstract IStorage CreateStorage(int capacity);
    
    [TestMethod]
    public void TestExceedCapacity()
    {
        IStorage skipListStorage = CreateStorage(10);
        for(int i = 0; i < 10; ++i) {skipListStorage.Insert(i, i);}
        Assert.ThrowsException<OutOfMemoryException>(() => skipListStorage.Insert(100, 100));
    }

    [TestMethod]
    public void TestJustWithinCapacity()
    {
        try
        {
            IStorage skipListStorage = CreateStorage(10);
            for(int i = 0; i < 10; ++i) {skipListStorage.Insert(i, i);}
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
            IStorage skipListStorage = CreateStorage(10);
            for(int i = 0; i < 10; ++i) {skipListStorage.Insert(i, i);}
            skipListStorage.Remove(0);
            skipListStorage.Insert(0, 1000);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [TestMethod]
    public void TestInsertAndRetrieve()
    {
        IStorage skipListStorage = CreateStorage(10);
        for(int i = 0; i < 10; ++i) {skipListStorage.Insert(i, i);}

        for (int i = 0; i < 10; ++i)
        {
            (int value, bool success) = skipListStorage.Retrieve(i);
            Assert.IsTrue(success);
            Assert.AreEqual(i, value);
        }
    }

    [TestMethod]
    public void TestRetrieveEmpty()
    {
        IStorage skipListStorage = CreateStorage(10);
        (_, bool success) = skipListStorage.Retrieve(0);
        Assert.IsFalse(success);
    }

    [TestMethod]
    public void TestInsertDuplicateKey()
    {
        IStorage skipListStorage = CreateStorage(10);
        for(int i = 0; i < 10; ++i) {skipListStorage.Insert(i, i);}
        for(int i = 0; i < 10; ++i) {skipListStorage.Insert(i, i + 100);}

        for (int i = 0; i < 10; ++i)
        {
            (int value, bool success) = skipListStorage.Retrieve(i);
            Assert.IsTrue(success);
            Assert.AreEqual(i + 100, value);
        }
    }

    [TestMethod]
    public void TestRemoveNonExistingKey()
    {
        try
        {
            IStorage skipListStorage = CreateStorage(10);
            skipListStorage.Remove(0);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}