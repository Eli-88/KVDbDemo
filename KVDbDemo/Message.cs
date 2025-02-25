namespace KVDbDemo;

public struct AddMsg
{
    public int Key { get; set; }
    public int Value { get; set; }
}

public struct RemoveMsg
{
    public int Key { get; set; }
}   

public struct GetMsg
{
    public int Key { get; set; }
}
