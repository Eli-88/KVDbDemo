using System.Net;
using System.Text;
using System.Text.Json;

namespace KVDbDemo;

internal static class Program
{
    private class Db
    {
        public Db()
        {
            _storage = new Storage(1 << 20);
            _lock = new ReaderWriterLockSlim();
        }

        public void Insert(int key, int value)
        {
            _lock.EnterWriteLock();
            _storage.Insert(key, value);
            _lock.ExitWriteLock();
        }

        public void Remove(int key)
        {
            _lock.EnterWriteLock();
            _storage.Remove(key);
            _lock.ExitWriteLock();
        }

        public (int, bool) Get(int key)
        {
            _lock.EnterReadLock();
            
            int value;
            bool success;
            (value, success) = _storage.Retrieve(key);
            
            _lock.ExitReadLock();
            
            return (value, success);
        }

        private Storage _storage;
        private ReaderWriterLockSlim _lock;
    }
    
    private static async Task HandleRequest(
        Db db,
        HttpListenerContext httpContext)
    {
        HttpListenerResponse response = httpContext.Response;
        HttpListenerRequest request = httpContext.Request;

        if (request.HasEntityBody)
        {
            string responseString;
            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string body = await reader.ReadToEndAsync();

                try
                {
                    switch (request.Url?.AbsolutePath.ToLower() ?? "invalid_path")
                    {
                        case "/add":
                        {
                            AddMsg msg = JsonSerializer.Deserialize<AddMsg>(body);
                            db.Insert(msg.Key, msg.Value);
                            responseString = "success";
                            response.StatusCode = 200;
                            break;
                        }
                        case "/remove":
                        {
                            RemoveMsg msg = JsonSerializer.Deserialize<RemoveMsg>(body);
                            db.Remove(msg.Key);
                            responseString = "success";
                            response.StatusCode = 200;
                            break;
                        }
                        case "/get":
                        {
                            GetMsg msg = JsonSerializer.Deserialize<GetMsg>(body);
                            (int value, bool success) = db.Get(msg.Key);
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
            
            await response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        response.OutputStream.Close();
    }
    
    
    public static async Task Main()
    {
        Db db = new Db();
        
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        
        bool running = true;
        while (running)
        {
            HttpListenerContext httpContext = await listener.GetContextAsync();
            await Task.Run(() => HandleRequest(db, httpContext));
        }
    }
}