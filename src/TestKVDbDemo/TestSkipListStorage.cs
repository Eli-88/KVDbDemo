﻿using KVDbDemo;
using KVDbDemo.Storage;

namespace TestKVDbDemo;

[TestClass]
public sealed class TestSkipListStorage: TestBaseStorage
{
    protected override IStorage CreateStorage(int capacity)
    {
        return new SkipListStorage(capacity);
    }
}