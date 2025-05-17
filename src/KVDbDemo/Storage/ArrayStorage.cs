namespace KVDbDemo.Storage;

public class ArrayStorage: IStorage
{
    private struct Node
    {
        public int Key;
        public int Value;
    }

    public ArrayStorage(int capacity)
    {
        _allocated = new Node[capacity];
        _capacity = capacity;
        _count = 0;
    }
    
    public (int Value, bool Success) Retrieve(int key)
    {
        for (int i = 0; i < _count; i++)
        {
            if (_allocated[i].Key == key)
            {
                return (_allocated[i].Value, true);
            }
        }
        
        return (-1, false);
    }

    public void Insert(int key, int value)
    {
        int idx = FindNode(key);
        if (idx != -1)
        {
            _allocated[idx].Value = value;
            return;
        }
        
        bool exceededCapacity = _count + 1 > _capacity;
        if (exceededCapacity)
        {
            throw new OutOfMemoryException();
        }
        _allocated[_count++] = new Node { Key = key, Value = value };
    }

    public void Remove(int key)
    {
        int idx = FindNode(key);
        if(idx == -1) return;

        _allocated[idx] = _allocated[_count - 1];
        --_count;
    }
    
    public void Dispose()
    {

    }

    private int FindNode(int key)
    {
        for (int i = 0; i < _count; i++)
        {
            if (_allocated[i].Key == key)
            {
                return i;
            }
        }

        return -1;
    }

    private int _count;
    private int _capacity;
    private Node[] _allocated;
    
}