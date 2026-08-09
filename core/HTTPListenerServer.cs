namespace csharp_http_server_1.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class HttpListenerServer
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Dictionary<string, Func<HttpListenerContext, Task>> _routes;


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
        response.ContentType = "application/json";
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions{
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await WriteResponse(response, json);
    }

    
}