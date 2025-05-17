using System.Diagnostics;

namespace KVDbDemo.Storage;

public class HashMapStorage : IStorage
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

        _allocated = new Node[_capacity];
        _count = 0;

        for (int i = 0; i < _capacity; i++)
        {
            _allocated[i].Used = false;
        }
    }

    public void Dispose()
    {
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
        int idx = -1;
        for (int i = 0; i < _capacity; i++)
        {
            if (_allocated[i].Key == key || _allocated[i].Used == false)
            {
                idx = i;
                break;
            }
            hash = (hash + 1) % _capacity;
        }
        
        Debug.Assert(idx != -1);
        ref Node node = ref _allocated[idx];
        
        if (node.Used == false)
        {
            if (_count == _maxCount) { throw new OutOfMemoryException(); }
            _count++;
        }
        node.Used = true;
        node.Key = key;
        node.Value = value;
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
    private Node[] _allocated;
}