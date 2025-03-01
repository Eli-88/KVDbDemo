using System.Net;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace KVDbDemo;

internal static class Program
{
    private static void RequestProcessingTask<T>(
        T storage, 
        ConcurrentQueue<HttpListenerContext> pending) where T : IStorage
    {
        while (true)
        {
            if (pending.TryDequeue(out var httpContext))
            {
                HttpListenerResponse response = httpContext.Response;
                HttpListenerRequest request = httpContext.Request;

                if (request.HasEntityBody)
                {
                    string responseString;
                    using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string body = reader.ReadToEnd();

                        try
                        {
                            switch (request.Url?.AbsolutePath.ToLower() ?? "invalid_path")
                            {
                                case "/add":
                                {
                                    AddMsg msg = JsonSerializer.Deserialize<AddMsg>(body);
                                    storage.Insert(msg.Key, msg.Value);
                                    responseString = "success";
                                    response.StatusCode = 200;
                                    break;
                                }
                                case "/remove":
                                {
                                    RemoveMsg msg = JsonSerializer.Deserialize<RemoveMsg>(body);
                                    storage.Remove(msg.Key);
                                    responseString = "success";
                                    response.StatusCode = 200;
                                    break;
                                }
                                case "/get":
                                {
                                    GetMsg msg = JsonSerializer.Deserialize<GetMsg>(body);
                                    (int value, bool success) = storage.Retrieve(msg.Key);
                                    responseString = success ? $"{value}" : "key not found";
                                    response.StatusCode = 200;
                                    break;
                                }
                                default:
                                {
                                    responseString = "404 Not Found";
                                    response.StatusCode = 404;
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            responseString = "400 Bad Request";
                            response.StatusCode = 400;
                        }
                    }
                    
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = responseBytes.Length;
                    
                    response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                }

                response.OutputStream.Close();
            }
        }
    }
    
    
    public static void Main()
    {
        ConcurrentQueue<HttpListenerContext> pending = new ConcurrentQueue<HttpListenerContext>();
        using SkipListStorage skipListStorage = new SkipListStorage(1 << 20);
        
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        new Thread(_ => { RequestProcessingTask(skipListStorage, pending); }).Start();
        while (true)
        {
            HttpListenerContext httpContext = listener.GetContext();
            pending.Enqueue(httpContext);
        }
    }
}