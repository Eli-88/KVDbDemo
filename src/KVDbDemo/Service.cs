using System.Collections.Concurrent;
using System.Net;
using System.Text;
using KVDbDemo.Storage;

namespace KVDbDemo;

public class Service(string host, ushort port)
{
    private readonly string _prefixUrl                                  = $"http://{host}:{port}/";
    private readonly ConcurrentQueue<HttpListenerContext> _pending      = new();
    private readonly Dictionary<string, IHandleRequest> _requestMapping = new();
    
    public void MapRequest(string path, IHandleRequest handleRequest) => _requestMapping[path] = handleRequest;
    
    public void Run(IStorage storage)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(_prefixUrl);
        listener.Start();

        new Thread(_ => { OnMessage(storage); }).Start();

        while (true)
        {
            HttpListenerContext httpContext = listener.GetContext();
            _pending.Enqueue(httpContext);
        }
    }

    private void OnMessage(IStorage storage)
    {
        while (true)
        {
            if (!_pending.TryDequeue(out var httpContext)) continue;

            HttpListenerResponse response   = httpContext.Response;
            HttpListenerRequest request     = httpContext.Request;

            string responseString;
            
            if (request.HasEntityBody)
            {
                using StreamReader reader   = new StreamReader(request.InputStream, request.ContentEncoding);
                string body                 = reader.ReadToEnd();
                
                try
                {
                    string? dispatchResponse    = OnDispatch(storage, request.Url?.AbsolutePath.ToLower() ?? "invalid path", body);
                    response.StatusCode         = dispatchResponse != null ? 200 : 404;
                    responseString              = dispatchResponse ?? "404 Not Found";
                }
                catch (Exception e)
                {
                    response.StatusCode = 400;
                    responseString      = "400 Bad Request";
                }
            }
            else
            {
                response.StatusCode = 400;
                responseString      = "400 Bad Request";
            }

            byte[] responseBytes        = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64    = responseBytes.Length;
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
            response.OutputStream.Close();
        }
    }
    
    internal string? OnDispatch(IStorage storage, string path, string body)
    {
        if (_requestMapping.TryGetValue(path, out var handleRequest))
        {
            return handleRequest.OnRequest(storage, body);
        }

        return null;
    }
}