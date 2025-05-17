using KVDbDemo.Storage;

namespace KVDbDemo;

public class RequestDispatcher: IRequestDispatcher
{
    private readonly Dictionary<string, IHandleRequest> _requestMapping = new Dictionary<string, IHandleRequest>();

    public void Add(string path, IHandleRequest handleRequest) => _requestMapping[path] = handleRequest;
    
    public string? OnDispatch(IStorage storage, string path, string body)
    {
        if (_requestMapping.TryGetValue(path, out var handleRequest))
        {
            return handleRequest.OnRequest(storage, body);
        }

        return null;
    }
}