using System.Diagnostics;

namespace KVDbDemo.Storage;

public unsafe class HashMapStorage : IStorage
{
    private struct Node
    {
        public bool Used;
        public int Key;
        public int Value;
    }

    public HashMapStorage(int capacity)
    {
        _maxCount = capacity;
        _capacity = PowerOfTwo(capacity);
        var buffer = OS.Mmap(IntPtr.Zero,
            (ulong)_capacity,
            OS.PROT_READ | OS.PROT_WRITE,
            OS.MAP_ANONYMOUS | OS.MAP_PRIVATE,
            -1,
            0);
        
        if(buffer == OS.MAP_FAIL) {throw new OutOfMemoryException();}
        _allocated = (Node*)buffer;
        _count = 0;

        for (int i = 0; i < _capacity; i++)
        {
            _allocated[i].Used = false;
        }
    }

    public void Dispose()
    {
        OS.Munmap((IntPtr)_allocated, (ulong)_capacity);
    }

    public (int Value, bool Success) Retrieve(int key)
    {
        int hash = Hash(key);
        for (int i = 0; i < _capacity; i++)
        {
            if (_allocated[i].Used == false) { return (-1, false); }
            if (_allocated[i].Key == key)    { return (_allocated[i].Value, true); }
            hash = (hash + 1) % _capacity;
        }
        return (-1, false);
    }

    public void Insert(int key, int value)
    {
        
        int hash = Hash(key);
        Node* node = null;
        for (int i = 0; i < _capacity; i++)
        {
            if (_allocated[i].Key == key || _allocated[i].Used == false)
            {
                node = &_allocated[i];
                break;
            }
            hash = (hash + 1) % _capacity;
        }
        
        Debug.Assert(node != null);

        if (node->Used == false)
        {
            if (_count == _maxCount) { throw new OutOfMemoryException(); }
            _count++;
        }
        node->Used = true;
        node->Key = key;
        node->Value = value;
    }

    public void Remove(int key)
    {
        int hash = Hash(key);
        for (int i = 0; i < _capacity; i++)
        {
            if(_allocated[i].Used == false) { break; }
            if(_allocated[i].Key == key)    {_allocated[i].Used = false; --_count; }
            hash = (hash + 1) % _capacity;
        }
    }

    private int PowerOfTwo(int value)
    {
        if(value == 0) return 1; 
        
        int p = 1;
        while (p < value)
        {
            p += p;
        }

        return p;
    }

    private int Hash(int key) => (_capacity - 1) & key;

    private int _count;
    private int _maxCount;
    private int _capacity;
    private Node* _allocated;
}