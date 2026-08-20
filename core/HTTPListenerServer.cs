namespace csharp_http_server_1.Core;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class HttpListenerServer
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Dictionary<string, Func<HttpListenerContext, Task>> _routes;

    private static int _totalRequests = 0;
    
    public HttpListenerServer(string [] prefixes)
    {   
        _listener = new HttpListener();
        _cancellationTokenSource = new CancellationTokenSource();
        _routes = new Dictionary<string, Func<HttpListenerContext, Task>>();


        //add URL prefixes for the server to listen on
        foreach (string prefix in prefixes)
        {
            _listener.Prefixes.Add(prefix);
        }

        SetUpDefaultRoutes();
    }


    //helper methods
    private async Task<string> ReadRequestBody(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
        {
            return string.Empty;
        }
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        return await reader.ReadToEndAsync();
    }

    private async Task WriteResponse(HttpListenerResponse response, string content)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer,0, buffer.Length);
        response.Close();
    }

    private async Task WriteJsonResponse(HttpListenerResponse response, object data)
    {   
        //tells browswer the response body is json formatted
        response.ContentType = "application/json";

        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions{
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await WriteResponse(response, json);
    }

    //Handlers for get requests
    private async Task HandleHomeRoute(HttpListenerContext context)
    {
        var html = @"
    <!DOCTYPE html>
    <html>
    <head>
        <title>HTTP Server</title>
        <style>
            body { font-family: Arial, sans-serif; margin: 40px; background: #121212; color: #fff; }
            .endpoint { background: #1e1e1e; padding: 15px; margin: 10px 0; border-left: 4px solid #007acc; border-radius: 4px; }
            a { color: #4ec9b0; text-decoration: none; font-weight: bold; }
            a:hover { text-decoration: underline; }
            button { background: #007acc; color: white; border: none; padding: 8px 12px; cursor: pointer; border-radius: 4px; }
            button:hover { background: #005999; }
            pre { background: #000; padding: 10px; border-radius: 4px; color: #0f0; max-height: 150px; overflow-y: auto; }
        </style>
    </head>
    <body>
        <h1>HTTP Server Running</h1>
        <h2>Available Endpoints:</h2>

        <div class='endpoint'>
            <strong>GET /</strong> - <a href='/'>Refresh Home Page</a>
        </div>

        <div class='endpoint'>
            <strong>GET /api/users</strong> - <a href='/api/users' target='_blank'>Click to View All Users</a>
        </div>

        <div class='endpoint'>
            <strong>GET /api/users/1</strong> - <a href='/api/users/1' target='_blank'>Click to View User #1</a>
        </div>

        <div class='endpoint'>
            <strong>POST /api/users</strong> - 
            <button onclick=""sendPost('/api/users', { name: 'Interactive User', email: 'click@example.com' })"">
                Test POST User
            </button>
        </div>

        <div class='endpoint'>
            <strong>POST /api/data</strong> - 
            <button onclick=""sendPost('/api/data', { click: 'success', status: 'live' })"">
                Test POST Data
            </button>
        </div>

        <h3>Interactive Response Output:</h3>
        <pre id='output'>Click a POST button above to see the response here...</pre>

        <script>
            async function sendPost(url, payload) {
                const res = await fetch(url, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                const data = await res.json();
                document.getElementById('output').textContent = JSON.stringify(data, null, 2);
            }
        </script>
    </body>
    </html>";

        context.Response.ContentType = "text/html";
        await WriteResponse(context.Response, html);
    }

    private async Task HandleGetUsers(HttpListenerContext context)
    {
        var users = new[]
        {
            new { Id = 1, Name = "Bruce Wayne", Email = "bruce@example.com" },
            new { Id = 2, Name = "Walter White", Email = "walt@example.com" },
            new { Id = 3, Name = "Bojack Horseman", Email = "boj@example.com" }
        };

        await WriteJsonResponse(context.Response, users);
    }

    private async Task HandleGetUserById(HttpListenerContext context)
    {
        var path = context.Request.Url.LocalPath;
        var segments = path.Split('/');

        if(segments.Length >= 4 && int.TryParse (segments[3], out int userId))
        {
            //default http status is 200 "ok" if valid path
            var user = new {Id = userId, Name = $"User{userId}", Email = $"user{userId}@example.com"};
            await WriteJsonResponse(context.Response, user);
        }
        else
        {   
            /* 
                set  HTTP Status 400 "Bad Request" if path didn't 
                contain enough segments or the ID wasn't a valid integer
            */
            context.Response.StatusCode = 400;
            await WriteJsonResponse(context.Response, new {error = "Invalid user ID"});
        }

    }

     //Handlers for post requests and adding new users
    private async Task HandleCreateUser(HttpListenerContext context)
    {   
        //Read post body
        try
        {
            string jsonString = await ReadRequestBody(context.Request);

            if (string.IsNullOrEmpty(jsonString))
            {
               context.Response.StatusCode = 400;
               await WriteJsonResponse(context.Response, new {error = "Request body is required!"});
               return; 
            }

            //parse JSON
            var userData = JsonSerializer.Deserialize<Dictionary<string,object>>(jsonString);

            //simulate creating user
            var newUser = new
            {
              Id = new Random().Next(1000, 9999),
              Name = userData.GetValueOrDefault("name", "Unknown").ToString(),
              Email = userData.GetValueOrDefault("email","unknown@example.com").ToString(),
              CreatedAt = DateTime.UtcNow
            };

            context.Response.StatusCode = 201;
            await WriteJsonResponse(context.Response, newUser);
            
        }
        catch(JsonException)
        {
           context.Response.StatusCode = 400;
           await WriteJsonResponse(context.Response, new {error = "Invalid JSON in request body"});
        }
    }

    private async Task HandlePostData(HttpListenerContext context)
    {
        try
        {
            string requestBody = await ReadRequestBody(context.Request);

            var responseData = new
            {
                message = "Data recieved successfully",
                recievedAt = DateTime.UtcNow,
                contentType = context.Request.ContentType,
                contentLength = context.Request.ContentLength64,
                data = requestBody

            };  

            await WriteJsonResponse(context.Response, responseData);

        }
        catch(Exception ex)
        {
            context.Response.StatusCode = 400;
            await WriteJsonResponse(context.Response, new {error = ex.Message});
        }
    }

    private void SetUpDefaultRoutes()
    {
        //GET routes
        _routes["GET /"] = HandleHomeRoute;
        _routes["GET /api/users"] = HandleGetUsers;
        _routes["GET /api/users/{id}"] = HandleGetUserById;

        //POST routes
        _routes["POST /api/users"] = HandleCreateUser;
        _routes["POST /api/data"] = HandlePostData;

    }


    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine("HTTP Server started on:");
        foreach (string prefix in _listener.Prefixes)
        {
            Console.WriteLine($" {prefix}");

        }

        //handle requests concurrently (as a server should)
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(HandleIncomingConnections());
        }

        await Task.WhenAll(tasks);

    }

    public async Task HandleIncomingConnections()
    {
       while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => ProcessRequest(context));
            }
            catch(ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting context: {ex.Message}");
            }
        } 
    }

    private async Task HandleNotFound(HttpListenerContext context)
    {
        context.Response.StatusCode = 404;
        await WriteJsonResponse(context.Response, new {error = "Endpoint not found"});
    }

    private async Task ProcessRequest(HttpListenerContext context)
    {   

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        int count = Interlocked.Increment(ref _totalRequests);

        try
        {
            var request = context.Request;
            var response = context.Response;

            AddCorsHeaders(response);

            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            

            string localPath = request.Url?.LocalPath ?? "/";
            string routeKey = $"{request.HttpMethod} {localPath}";

            if (_routes.TryGetValue(routeKey, out var handler))
            {
                await handler(context);
            }
            else if (IsParameterizedRoute(request, out var paramHandler))
            {
                await paramHandler!(context);
            }
            else
            {
                await HandleNotFound(context);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing request: {ex.Message}");
            try
            {
                context.Response.StatusCode = 500;
                await WriteJsonResponse(context.Response, new { error = "Internal server error" });
            }
            catch
            {
                Console.WriteLine("response is already closed");
            }
        }
        finally
        {
            
            stopwatch.Stop();
            
            string method = context.Request.HttpMethod;
            string path = context.Request.Url?.LocalPath ?? "/";
            int status = context.Response.StatusCode;

            Console.WriteLine($"Count: {count:D4} | [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] | {method,-4} {path,-20}" +
            $"-> Status: {status} [{stopwatch.Elapsed.TotalMilliseconds:F1}ms]");
        }
    }
    

    private bool IsParameterizedRoute(HttpListenerRequest request, out Func<HttpListenerContext, Task>? handler)
    {
        handler = null;
        var path = request.Url.LocalPath;
        var method = request.HttpMethod;

        if(method == "GET" && path.StartsWith("/api/users/") && path.Length > "/api/users".Length)
        {   
            handler = HandleGetUserById;
            return true;
        }

        return false;
    }

    private void AddCorsHeaders(HttpListenerResponse response)
    {
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization"); 
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _listener?.Stop();
        _listener?.Close();
   }

}