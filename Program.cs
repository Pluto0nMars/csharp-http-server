using System.Net;
using csharp_http_server_1.Core;


class Program
{
    static async Task Main(string[] args)
    {
        string[] prefixes = {"http://localhost:8080/"};
        var server =  new HttpListenerServer(prefixes);

        Console.WriteLine("Starting HTTP Server...");
        Console.WriteLine("Press 'q' to quit");

        var serverTask = server.StartAsync();

        while (true)
        {
            var key = Console.ReadKey(true);
            if(key.KeyChar == 'q' || key.KeyChar == 'Q')
            {
                break;
            }
        }

        Console.WriteLine("Stopping server....");
        server.Stop();

        try
        {
            await serverTask;
        }
        catch(OperationCanceledException ex)
        {
            Console.WriteLine($"Server cancelled cleanly: {ex.Message}");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Server stopped with an error: {ex.Message}");
        }

        Console.WriteLine("Server stopped successfully!");  
        
    }
}
