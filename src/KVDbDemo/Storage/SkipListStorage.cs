using System.Diagnostics;

namespace KVDbDemo;

public unsafe class SkipListStorage : IStorage
{
    private const int MAX_LEVEL = 8;
    private const int INVALID_INDEX = -1;
    
    struct Node
    {
        public bool Used;
        public byte Height;
        public int Key;
        public int Value;
        public fixed int NextIndex[MAX_LEVEL];
    }

    public SkipListStorage(int capacity)
    {
        _capacity = capacity + 1;
        IntPtr allocated = OS.Mmap(
            IntPtr.Zero,
            (ulong)(_capacity * sizeof(Node)),
            OS.PROT_READ | OS.PROT_WRITE,
            OS.MAP_PRIVATE | OS.MAP_ANONYMOUS,
            -1,
            0);
        if (allocated == OS.MAP_FAIL) { throw new OutOfMemoryException("mmap failed"); }
        _allocated = (Node*)allocated;

        
        // allocate a dummy head that never be free
        _offset = 1;
        Node* dummyHead = &_allocated[0];
        dummyHead->Used = true;
        dummyHead->Height = MAX_LEVEL;
        dummyHead->Key = -1;
        dummyHead->Value = -1;
        for(int i = 0; i < MAX_LEVEL; i++) {dummyHead->NextIndex[i] = INVALID_INDEX;}
    }

    public (int Value, bool Success) Retrieve(int key)
    {
            Span<int> prevNodeIndexes = stackalloc int[MAX_LEVEL];
            Node* curr = FindNode(key, prevNodeIndexes);
            if(curr == null) { return (-1, false); }
        
            Debug.Assert(curr->Used);
            return (curr->Value, true);
    }

    public void Insert(int key, int value)
    {
        Span<int> prevNodeIndexes = stackalloc int[MAX_LEVEL];
        Node* node = FindNode(key, prevNodeIndexes);
        if (node != null)
        {
            node->Value = value;
            return;
        }

        Node* newNode = Allocate(key, value);
        for(int i = 0; i < MAX_LEVEL; i++) { newNode->NextIndex[i] = INVALID_INDEX; }

        for (var i = 0; i < newNode->Height; i++)
        {
            Node* prevNode = IndexToNode(prevNodeIndexes[i]);

            if (prevNode != null)
            {
                newNode->NextIndex[i] = prevNode->NextIndex[i];
                prevNode->NextIndex[i] = NodeToIndex(newNode);
            }
        }
    }

    public void Remove(int key)
    {
        Span<int> prevNodeIndexes = stackalloc int[MAX_LEVEL];
        Node* node = FindNode(key, prevNodeIndexes);
        
        if (node == null || node->Key != key) { return; }

        int height = node->Height;
        for (int i = 0; i < height; i++)
        {
            Node* prevNode = IndexToNode(prevNodeIndexes[i]);
            if (prevNode != null)
            {
                prevNode->NextIndex[i] = node->NextIndex[i];
            }
        }
        
        Free(node);
    }
    
    
    private Node* FindNode(int key, Span<int> prevNodeIndexes)
    {
        Node* found = null;
        
        Node* curr = &_allocated[0];
        int level = MAX_LEVEL - 1;
        
        for(int i = 0; i < MAX_LEVEL; i++) { prevNodeIndexes[i] = INVALID_INDEX; }

        bool levelRemaining = true;
        while (levelRemaining)
        {
            Node* next = IndexToNode(curr->NextIndex[level]);
            prevNodeIndexes[level] = NodeToIndex(curr);

            if (next == null)
            {
                --level;
            }
            else
            {
                if (key == next->Key)     { found = next; --level; }
                else if (next->Key > key) { --level; }
                else                      { curr = next; }
            }
            
            levelRemaining = (level >= 0);
        }
        
        return found;
    }
    
    #region Helper Function

    private Node* Allocate(int key, int value)
    {
        Node* node = null;
        if (_freeCount > 0)
        {
            --_freeCount;
            for (int i = 0; i < _capacity; i++)
            {
                if (!_allocated[i].Used)
                {
                    node = &_allocated[i];
                    break;
                }
            }

        }
        else
        {
            if(_offset + 1 > _capacity) { throw new OutOfMemoryException(); }
            node =  &_allocated[_offset++];
        }

        node->Used = true;
        node->Key = key;
        node->Value = value;
        node->Height = Height();
        
        return node;
    }

    private void Free(Node* node)
    {
        ++_freeCount;
        node->Used = false;
        for(int i = 0; i < MAX_LEVEL; ++i) {node->NextIndex[i] = INVALID_INDEX;}
    }

    private Node* IndexToNode(int index) { return index != INVALID_INDEX ? &_allocated[index] : null; }
    private int NodeToIndex(Node* node) { return node != null ? (int)(node - _allocated) : INVALID_INDEX; } 
    private byte Height() { return (byte)(_lastRandomIndex++ % (MAX_LEVEL - 1) + 1); }

    #endregion
    
    #region Field

    private int _offset = 0;
    private int _freeCount = 0;
    private int _lastRandomIndex = 0;
    private readonly int _capacity;
    private readonly Node* _allocated;

    #endregion
}