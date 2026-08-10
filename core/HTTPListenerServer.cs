namespace csharp_http_server_1.Core;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
                body { font-family: Arial, sans-serif; margin: 40px; }
                .endpoint { background: #f5f5f5; padding: 10px; margin: 10px 0; border-left: 4px solid #007acc; }
            </style>
        </head>
        <body>
            <h1>HTTP Server Running</h1>
            <h2>Available Endpoints in this server:</h2>
            <div class='endpoint'><strong>GET /</strong> - This page</div>
            <div class='endpoint'><strong>GET /api/users</strong> - Get all users</div>
            <div class='endpoint'><strong>GET /api/users/{id}</strong> - Get user by ID</div>
            <div class='endpoint'><strong>POST /api/users</strong> - Create new user</div>
            <div class='endpoint'><strong>POST /api/data</strong> - Post JSON data</div>
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
}