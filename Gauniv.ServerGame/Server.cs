using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Gauniv.GameServer
{
    public class Server
    {
        private const int MaxPlayers = 2;
        private TcpListener listener;
        private List<Player> players = new List<Player>();
        private CancellationTokenSource cancellationTokenSource;

        public async Task StartServer(int port = 5000, CancellationToken cancellationToken = default)
        {
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            listener = new TcpListener(IPAddress.Any, port);

            try
            {
                listener.Start();
                Console.WriteLine($"[DEBUG] Server started on port {port}");

                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Console.WriteLine("[DEBUG] Waiting for client connection...");

                    var acceptClientTask = listener.AcceptTcpClientAsync();

                    await using (cancellationTokenSource.Token.Register(() => listener.Stop()))
                    {
                        try
                        {
                            TcpClient client = await acceptClientTask;

                            if (players.Count >= MaxPlayers)
                            {
                                Console.WriteLine("[DEBUG] Maximum players reached. Disconnecting client.");
                                client.Close();
                                continue;
                            }

                            var player = new Player(client, this);
                            players.Add(player);
                            Console.WriteLine($"[DEBUG] Player added with SessionID: {player.SessionID}");

                            _ = Task.Run(() => player.HandleClient(), cancellationTokenSource.Token);
                        }
                        catch (ObjectDisposedException)
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                listener.Stop();
                foreach (var player in players.ToList())
                {
                    await player.SendMessageAsync("SERVER_SHUTDOWN");
                }
                players.Clear();
            }
        }

        public void Stop()
        {
            cancellationTokenSource?.Cancel();
        }

        public async Task<bool> AuthenticatePlayer(Player player, string username, string password)
        {
            if (username == "b" && password == "bb")
            {
                Console.WriteLine("[DEBUG] Authentication successful for player.");
                return true;
            }
            else
            {
                Console.WriteLine("[DEBUG] Authentication failed for player.");
                return false;
            }
        }

        public async Task<bool> ValidatePlayerName(Player player, string playerName)
        {
            if (players.Any(p => p.Name == playerName))
            {
                return false;
            }

            player.SetPlayerName(playerName);
            Console.WriteLine($"[DEBUG] Name '{playerName}' assigned to player with SessionID: {player.SessionID}");
            return true;
        }

        // Method to mark player as ready and check if all players are ready
        public async Task PlayerReady(Player player)
        {
            player.IsReady = true;
            Console.WriteLine($"[DEBUG] Player {player.SessionID} is ready.");

            // Check if all players are ready
            if (players.All(p => p.IsReady))
            {
                Console.WriteLine("[DEBUG] All players are ready. Starting game...");

                // Broadcast `ALL_READY` or `START_GAME` to all players
                foreach (var p in players)
                {
                    await p.SendMessageAsync("START_GAME");
                }
            }
        }
    }
}
