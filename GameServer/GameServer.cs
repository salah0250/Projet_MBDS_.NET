using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using MessagePack;
using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;

namespace GameServer
{
    public class GameServer
    {
        private TcpListener listener;
        private readonly List<GameRoom> gameRooms;
        private readonly Dictionary<string, Player> connectedPlayers;
        private bool isRunning;
        private readonly int gridSize = 8; // Taille du damier N*N

        public GameServer(int port = 5000)
        {
            listener = new TcpListener(IPAddress.Any, port);
            gameRooms = new List<GameRoom>();
            connectedPlayers = new Dictionary<string, Player>();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                listener.Start();
                isRunning = true;
                Console.WriteLine("Game server started on port 5000...");

                while (isRunning && !cancellationToken.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync(cancellationToken);
                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
            finally
            {
                listener.Stop();
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                var player = await AuthenticatePlayer(client);
                // ajoute la partie pour recevoir les messages du joueur Task<GameMessage> ReceiveMessageAsync()
            


               
                  if (player != null)
        {
            connectedPlayers[player.Name] = player;
            Console.WriteLine($"Player {player.Name} connected.");
            await HandlePlayerCommunication(player);
        }
        else
        {
            Console.WriteLine("Authentication failed. Closing connection.");
            client.Close();
        }
              
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client handler error: {ex.Message}");
            }
            finally
            {
                client?.Close();
            }
        }

       private async Task<Player> AuthenticatePlayer(TcpClient client)
{
    var stream = client.GetStream();
    var buffer = new byte[4096];

    try
    {
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead > 0)
        {
            var authData = MessagePackSerializer.Deserialize<AuthenticationData>(buffer.AsMemory(0, bytesRead));
            Console.WriteLine($"Authentication attempt: PlayerName = {authData.PlayerName}, Token = {authData.Token}");

            // Vérifier si le nom est déjà pris
            if (connectedPlayers.ContainsKey(authData.PlayerName))
            {
                Console.WriteLine($"Player name {authData.PlayerName} is already taken.");
                await SendResponseAsync(client, new NameValidationResponse { IsValid = false });
                return null;
            }
            else
            {
                Console.WriteLine($"Player name {authData.PlayerName} is available.");
                await SendResponseAsync(client, new NameValidationResponse { IsValid = true });
                return new Player(authData.PlayerName, client);
            }
        }
        else
        {
            Console.WriteLine("No data received during authentication.");
            await SendResponseAsync(client, new NameValidationResponse { IsValid = false });
            return null;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Authentication error: {ex.Message}");
        await SendResponseAsync(client, new NameValidationResponse { IsValid = false });
        return null;
    }
}
        private bool ValidateToken(string token)
        {
            // Implémenter la validation du token ici
            return true; // Pour test, à remplacer par une vraie validation
        }

       private async Task SendResponseAsync(TcpClient client, object response)
{
    try
    {
        var stream = client.GetStream();
        Console.WriteLine("Données envoyées (objet) : " + response);

        var data = MessagePackSerializer.Serialize(response);

        // Journaliser les données sérialisées
        Console.WriteLine("Données envoyées (byte array) : " + BitConverter.ToString(data));

        await stream.WriteAsync(data, 0, data.Length);
        await stream.FlushAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Send response error: {ex.Message}");
        throw;
    }
}

      private async Task HandlePlayerCommunication(Player player)
{
    try
    {
        while (player.IsConnected)
        {
            var message = await player.ReceiveMessageAsync();
            Console.WriteLine($"Message received from {player.Name}: {message.Type}");
            await ProcessPlayerMessage(player, message);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Player communication error: {ex.Message}");
    }
    finally
    {
        if (connectedPlayers.ContainsKey(player.Name))
        {
            connectedPlayers.Remove(player.Name);
        }
        player.Disconnect();
    }
}

  private async Task ProcessPlayerMessage(Player player, GameMessage message)
{
    var room = gameRooms.Find(r => r.HasPlayer(player));

    // Journaliser les données du message reçu
    Console.WriteLine($"Message reçu de {player.Name} : {message.Type}");
    Console.WriteLine($"Gamemaster : {message.Gamemaster}");
    Console.WriteLine($"CellPosition : ({message.CellPosition.X}, {message.CellPosition.Y})");
    Console.WriteLine($"PlayersList : {string.Join(", ", message.PlayersList)}");
    Console.WriteLine($"Rankings Count : {message.Rankings.Count}");

    switch (message.Type)
    {
        case GameMessageType.JoinRoom:
            if (room == null)
            {
                room = GetOrCreateGameRoom();
                await room.AddPlayerAsync(player);
            }
            break;

        case GameMessageType.Ready:
            Console.WriteLine($"Received Ready message from {player.Name}");
            if (room != null)
            {
                await room.SetPlayerReadyAsync(player);
            }
            break;

        case GameMessageType.CellSelected:
            if (room != null)
            {
                var godotVector = message.CellPosition.ToGodotVector();
                await room.ProcessCellSelectionAsync(player, godotVector);
            }
            break;
    }
}
 private async Task BroadcastMessageToRoom(GameRoom room, GameMessage message)
    {
        try
        {
            Console.WriteLine($"Broadcasting message to room:");
            Console.WriteLine($"Type: {message.Type}");
            Console.WriteLine($"Gamemaster: {message.Gamemaster}");
            Console.WriteLine($"Players: {string.Join(", ", message.PlayersList ?? new List<string>())}");
            
            var serializedData = MessagePackSerializer.Serialize(message);
            Console.WriteLine($"Serialized data length: {serializedData.Length}");
            Console.WriteLine($"Serialized data: {BitConverter.ToString(serializedData)}");

            var tasks = room.GetPlayers().Select(p => p.SendMessageAsync(message));
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error broadcasting message: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

        private GameRoom GetOrCreateGameRoom()
        {
            var availableRoom = gameRooms.Find(r => !r.IsGameStarted && r.PlayerCount < 4);
            if (availableRoom == null)
            {
                availableRoom = new GameRoom(gridSize);
                gameRooms.Add(availableRoom);
            }
            return availableRoom;
        }
    }

    public class GameRoom
    {
        private readonly List<Player> players = new();
        private readonly Dictionary<Player, bool> readyStatus = new();
        public IEnumerable<Player> GetPlayers() => players;

        private readonly Dictionary<Player, DateTime> playerReactionTimes = new();
        private Player gamemaster;
        private Vector2 selectedCell;
        private bool isGameStarted;
        private readonly int gridSize;

        public bool IsGameStarted => isGameStarted;
        public int PlayerCount => players.Count;

        public GameRoom(int gridSize)
        {
            this.gridSize = gridSize;
        }

        public bool HasPlayer(Player player) => players.Contains(player);

        public async Task AddPlayerAsync(Player player)
        {
            if (!players.Contains(player))
            {
                players.Add(player);
                readyStatus[player] = false;
                await BroadcastPlayerListAsync();
            }
        }

        public async Task SetPlayerReadyAsync(Player player)
{
    if (!readyStatus.ContainsKey(player)) return;

    readyStatus[player] = true;
    Console.WriteLine($"{player.Name} is ready.");

    // Créer un message de confirmation avec toutes les propriétés nécessaires
    var readyConfirmation = new GameMessage
    {
        Type = GameMessageType.Ready,
        Gamemaster = player.Name,
        PlayersList = players.Select(p => p.Name).ToList(),
        Rankings = new List<PlayerRanking>(),
        CellPosition = new NetworkVector2(0, 0)
    };

    // Envoyer la confirmation à tous les joueurs
    await BroadcastMessageAsync(readyConfirmation);

    if (AreAllPlayersReady() && players.Count >= 2)
    {
        Console.WriteLine("All players are ready. Starting the game...");
        await StartGameAsync();
    }
}

         private bool AreAllPlayersReady()
    {
        return readyStatus.Values.All(status => status);
    }

      private async Task StartGameAsync()
{
    var message = new GameMessage
    {
        Type = GameMessageType.GameStart,
        Gamemaster = players.First().Name // Assigner le premier joueur comme Gamemaster
    };
    await BroadcastMessageAsync(message);
}
        private Player SelectGameMaster()
        {
            var random = new Random();
            return players[random.Next(players.Count)];
        }

        public async Task ProcessCellSelectionAsync(Player player, Vector2 cellPosition)
        {
            if (!IsValidCellPosition(cellPosition)) return;

            if (player == gamemaster && isGameStarted)
            {
                selectedCell = cellPosition;
                await BroadcastSelectedCellAsync();
                StartPlayerReactionTracking();
            }
            else if (isGameStarted && cellPosition == selectedCell)
            {
                RecordPlayerReaction(player);
                if (HaveAllPlayersResponded())
                {
                    await ProcessGameResultsAsync();
                }
            }
        }

        private bool IsValidCellPosition(Vector2 position)
        {
            return position.X >= 0 && position.X < gridSize &&
                   position.Y >= 0 && position.Y < gridSize;
        }

        private void StartPlayerReactionTracking()
        {
            playerReactionTimes.Clear();
            foreach (var player in players.Where(p => p != gamemaster))
            {
                playerReactionTimes[player] = DateTime.Now;
            }
        }

        private void RecordPlayerReaction(Player player)
        {
            if (playerReactionTimes.ContainsKey(player))
            {
                var reactionTime = DateTime.Now - playerReactionTimes[player];
                playerReactionTimes[player] = DateTime.Now.AddMilliseconds(-reactionTime.TotalMilliseconds);
            }
        }

        private bool HaveAllPlayersResponded() =>
            players.Where(p => p != gamemaster)
                  .All(p => playerReactionTimes.ContainsKey(p));

        private async Task ProcessGameResultsAsync()
        {
            var initialRankings = playerReactionTimes
                .OrderBy(kvp => kvp.Value)
                .Select((kvp, index) => (Player: kvp.Key, Position: index + 1))
                .ToList();

            var finalRankings = new List<(Player Player, int Position)>();

            // Vérifier l'éligibilité et ajuster les positions
            foreach (var ranking in initialRankings)
            {
                if (IsEligible(ranking.Position, ranking.Player.Name))
                {
                    finalRankings.Add(ranking);
                }
            }

            // Réajuster les positions après exclusions
            for (int i = 0; i < finalRankings.Count; i++)
            {
                finalRankings[i] = (finalRankings[i].Player, i + 1);
            }

            await BroadcastGameResultsAsync(finalRankings);
            ResetGame();
        }

        private bool IsEligible(int pos, string name)
        {
            Stopwatch sw = new();
            sw.Start();
            using (ECDsa key = ECDsa.Create())
            {
                key.GenerateKey(ECCurve.NamedCurves.nistP521);
                int t = 5000 / pos;
                var k = new byte[t];
                var d = Encoding.UTF8.GetBytes(name);
                byte[] combined = new byte[d.Length + sizeof(int)];
                d.CopyTo(combined, 0);
                BitConverter.GetBytes(pos).CopyTo(combined, d.Length);

                for (int i = 0; i < t; i++)
                {
                    var s = key.SignData(combined, HashAlgorithmName.SHA512);
                    k[i] = s[i % s.Length];
                }
                var res = key.SignData(k, HashAlgorithmName.SHA512);
                sw.Stop();
                Console.WriteLine($"{pos} {sw.ElapsedMilliseconds} {res}");
                return res[(int)Math.Truncate(res.Length / 4.0)] > 0x7F;
            }
        }

        private void ResetGame()
        {
            isGameStarted = false;
            gamemaster = null;
            selectedCell = Vector2.Zero;
            playerReactionTimes.Clear();
            foreach (var player in players)
            {
                readyStatus[player] = false;
            }
        }

       private async Task BroadcastMessageAsync(GameMessage message)
        {
            try
            {
                Console.WriteLine("=== Broadcasting Message ===");
                Console.WriteLine($"Type: {message.Type}");
                Console.WriteLine($"Gamemaster: {message.Gamemaster}");
                Console.WriteLine($"Players: {string.Join(", ", message.PlayersList ?? new List<string>())}");
                Console.WriteLine($"CellPosition: ({message.CellPosition.X}, {message.CellPosition.Y})");
                Console.WriteLine($"Rankings: {message.Rankings?.Count ?? 0} items");

                var serializedData = MessagePackSerializer.Serialize(message);
                Console.WriteLine($"Serialized data length: {serializedData.Length}");
                Console.WriteLine($"Serialized data: {BitConverter.ToString(serializedData)}");

                foreach (var player in players)
                {
                    try
                    {
                        await player.SendMessageAsync(message);
                        Console.WriteLine($"Message sent to player: {player.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send to player {player.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Broadcasting error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }


        private async Task BroadcastPlayerListAsync()
        {
            var message = new GameMessage
            {
                Type = GameMessageType.PlayerList,
                PlayersList = players.Select(p => p.Name).ToList()
            };
            await BroadcastMessageAsync(message);
        }

        private async Task BroadcastGameStartAsync()
        {
            var message = new GameMessage
            {
                Type = GameMessageType.GameStart,
                Gamemaster = gamemaster.Name
            };
            await BroadcastMessageAsync(message);
        }

        private async Task BroadcastSelectedCellAsync()
        {
            var message = new GameMessage
            {
                Type = GameMessageType.CellSelected,
                CellPosition = selectedCell
            };
            await BroadcastMessageAsync(message);
        }

        private async Task BroadcastGameResultsAsync(List<(Player Player, int Position)> rankings)
        {
            var message = new GameMessage
            {
                Type = GameMessageType.GameResults,
                Rankings = rankings.Select(r => new PlayerRanking
                {
                    PlayerName = r.Player.Name,
                    Position = r.Position,
                    IsEligible = true
                }).ToList()
            };
            await BroadcastMessageAsync(message);
        }
    }

    [MessagePackObject]
    public class Player
    {
        [Key(0)]
        public string Name { get; }

        private readonly TcpClient client;
        private readonly NetworkStream stream;

        public bool IsConnected => client?.Connected ?? false;

        public Player(string name, TcpClient client)
        {
            Name = name;
            this.client = client;
            stream = client.GetStream();
        }

public async Task<GameMessage> ReceiveMessageAsync()
{
    var buffer = new byte[4096];
    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
    if (bytesRead > 0)
    {
        using (var memoryStream = new MemoryStream(buffer, 0, bytesRead))
        {
            // Journaliser les données reçues avant la désérialisation
            Console.WriteLine("Données reçues (byte array) : " + BitConverter.ToString(buffer, 0, bytesRead));

            try
            {
                var message = MessagePackSerializer.Deserialize<GameMessage>(memoryStream);
                Console.WriteLine($"Message désérialisé : {message.Type}");
                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de désérialisation : {ex.Message}");
                Console.WriteLine($"Stack Trace : {ex.StackTrace}");
                Console.WriteLine($"Données reçues (byte array) : {BitConverter.ToString(buffer, 0, bytesRead)}");
                throw;
            }
        }
    }
    else
    {
        throw new Exception("Aucune donnée reçue");
    }
}

        public async Task SendMessageAsync(GameMessage message)
    {
        try
        {
            Console.WriteLine($"Sending message to player {Name}:");
            Console.WriteLine($"Type: {message.Type}");
            Console.WriteLine($"Gamemaster: {message.Gamemaster}");

            var data = MessagePackSerializer.Serialize(message);
            Console.WriteLine($"Serialized data length: {data.Length}");
            Console.WriteLine($"Serialized data: {BitConverter.ToString(data)}");

            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to player {Name}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

        public void Disconnect()
        {
            stream?.Dispose();
            client?.Close();
        }
    }

    [MessagePackObject]
public class GameMessage
{
    [Key(0)]
    public GameMessageType Type { get; set; }

    [Key(1)]
    public string Gamemaster { get; set; }

    [Key(2)]
    public NetworkVector2 CellPosition { get; set; }

    [Key(3)]
    public List<string> PlayersList { get; set; }

    [Key(4)]
    public List<PlayerRanking> Rankings { get; set; }

    public GameMessage()
    {
        PlayersList = new List<string>();
        Rankings = new List<PlayerRanking>();
        CellPosition = new NetworkVector2();
    }
}

public enum GameMessageType
{
    JoinRoom,
    Ready,
    GameStart,
    CellSelected,
    PlayerList,
    GameResults
}

[MessagePackObject]
public struct NetworkVector2
{
    [Key(0)]
    public int X { get; set; }
    
    [Key(1)]
    public int Y { get; set; }

    public NetworkVector2(int x, int y)
    {
        X = x;
        Y = y;
    }

    // Convertir depuis/vers le Vector2 de Godot
    public static NetworkVector2 FromGodotVector(Vector2 v)
    {
        return new NetworkVector2((int)v.X, (int)v.Y);
    }

    public Vector2 ToGodotVector()
    {
        return new Vector2(X, Y);
    }

        public static implicit operator NetworkVector2(Vector2 v)
        {
            throw new NotImplementedException();
        }
    }

[MessagePackObject]
public class PlayerRanking
{
    [Key(0)]
    public string PlayerName { get; set; } = string.Empty;
    
    [Key(1)]
    public int Position { get; set; }
    
    [Key(2)]
    public bool IsEligible { get; set; }
}
    [MessagePackObject]
    public class AuthenticationData
    {
        [Key(0)]
        public string PlayerName { get; set; }

        [Key(1)]
        public string Token { get; set; }
    }

    [MessagePackObject]
    public class NameValidationResponse
    {
        [Key(0)]
        public bool IsValid { get; set; }
    }

    

    public struct Vector2
    {
        [Key(0)]
        public int X { get; set; }
        [Key(1)]
        public int Y { get; set; }

        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new Vector2(0, 0);

        public static bool operator ==(Vector2 a, Vector2 b) =>
            a.X == b.X && a.Y == b.Y;

        public static bool operator !=(Vector2 a, Vector2 b) =>
            !(a == b);
    }
}
