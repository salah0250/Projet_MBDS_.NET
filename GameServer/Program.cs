using GameServer;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new GameServer.GameServer();

        // Utiliser un CancellationTokenSource pour gérer l'arrêt proprement
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            Console.WriteLine("Starting server...");
            await server.StartAsync(cts.Token);
            Console.WriteLine("Server started.");

            // Attendre que le token soit annulé pour garder l'application en cours d'exécution
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Server shutdown requested.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Server stopped.");
        }
    }
}
