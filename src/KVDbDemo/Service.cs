using System.Collections.Concurrent;
using System.Net;
using System.Text;
using KVDbDemo.Storage;

namespace KVDbDemo;

public class Service(string host, ushort port, IRequestDispatcher requestDispatcher)
{
    private readonly string _prefixUrl                               = $"http://{host}:{port}/";
    private readonly ConcurrentQueue<HttpListenerContext> _pending   = new();

    public void Run(IStorage storage)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(_prefixUrl);
        listener.Start();

        new Thread(_ => { ProcessTask(storage); }).Start();

        while (true)
        {
            HttpListenerContext httpContext = listener.GetContext();
            _pending.Enqueue(httpContext);
        }
    }

    private void ProcessTask(IStorage storage)
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
                    string? dispatchResponse    = requestDispatcher.OnDispatch(storage, request.Url?.AbsolutePath.ToLower() ?? "invalid path", body);
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
}