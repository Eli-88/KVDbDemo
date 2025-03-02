namespace KVDbDemo.Storage;

public unsafe class ArrayStorage: IStorage, IDisposable
{
    private struct Node
    {
        public int Key;
        public int Value;
    }

    public ArrayStorage(int capacity)
    {
        IntPtr allocated = OS.Mmap(
            IntPtr.Zero,
            (ulong)(capacity * sizeof(Node)),
            OS.PROT_READ | OS.PROT_WRITE,
            OS.MAP_PRIVATE | OS.MAP_ANONYMOUS,
            -1,
            0);

        if (allocated == OS.MAP_FAIL) throw new OutOfMemoryException();
        _allocated = (Node*)allocated;
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
        bool found = false;
        for (int i = 0; i < _count; i++)
        {
            if (_allocated[i].Key == key)
            {
                _allocated[i].Value = value;
                found = true;
                break;
            }
        }

        
        bool exceededCapacity = _count + 1 > _capacity;
        if (!found && exceededCapacity)
        {
            throw new OutOfMemoryException();
        }

        if (!found)
        {
            _allocated[_count++] = new Node { Key = key, Value = value };
        }
    }

    public void Remove(int key)
    {
        int idx = -1;
        for (int i = 0; i < _count; i++)
        {
            if (_allocated[i].Key == key)
            {
                idx = i;
            }
        }

        if (idx != -1)
        {
            _allocated[idx] = _allocated[_count - 1];
            --_count;
        }
    }
    
    public void Dispose()
    {
        OS.Munmap((IntPtr)_allocated, (ulong)_capacity);
    }

    private int _count;
    private int _capacity;
    private Node* _allocated;
    
}