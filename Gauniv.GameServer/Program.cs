using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gauniv.GameServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                int port = 5000;
                using var cancellationTokenSource = new CancellationTokenSource();
                var server = new Gauniv.GameServer.Server(); // Changé GameServer en Server

                // Gestion de l'arrêt propre du serveur
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true; // Empêche l'arrêt immédiat
                    Console.WriteLine("\nArrêt du serveur en cours...");
                    cancellationTokenSource.Cancel();
                };

                Console.WriteLine($"Démarrage du serveur de jeu sur le port {port}...");
                Console.WriteLine("Appuyez sur Ctrl+C pour arrêter le serveur.");

                try
                {
                    await server.StartServer(port);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Le serveur a été arrêté proprement.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'exécution du serveur : {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur fatale : {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Appuyez sur Entrée pour fermer...");
                Console.ReadLine();
            }
        }
    }
}