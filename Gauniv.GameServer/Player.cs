using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Gauniv.GameServer
{
    public class Player
    {
        private TcpClient client;
        private NetworkStream stream;
        private Server server;

        public string Name { get; private set; }
        public string SessionID { get; private set; }
        public bool IsReady { get; set; }

        public Player(TcpClient client, Server server)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.stream = client.GetStream();
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            SessionID = Guid.NewGuid().ToString();
        }

        public async Task HandleClient()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // Client disconnected

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    Console.WriteLine($"[DEBUG] Received message: {message}");
                    await ProcessCommand(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error in HandleClient: {ex.Message}");
            }
            finally
            {
                stream.Close();
                client.Close();
                Console.WriteLine("[DEBUG] Client disconnected.");
            }
        }

        private async Task ProcessCommand(string message)
        {
            var parts = message.Split(' ');

            switch (parts[0])
            {
                case "LOGIN":
                    if (parts.Length == 3)
                    {
                        string username = parts[1];
                        string password = parts[2];
                        bool isAuthenticated = await server.AuthenticatePlayer(this, username, password);
                        string response = isAuthenticated ? "AUTH_SUCCESS" : "AUTH_FAILED";
                        await SendMessageAsync(response);
                    }
                    break;

                case "NAME":
                    if (parts.Length == 2)
                    {
                        string playerName = parts[1];
                        bool nameIsValid = await server.ValidatePlayerName(this, playerName);
                        if (nameIsValid)
                        {
                            SetPlayerName(playerName); // Ensure this method is implemented
                        }
                        string response = nameIsValid ? "NAME_OK" : "NAME_TAKEN";
                        await SendMessageAsync(response);
                    }
                    break;

                case "READY":
                    await server.PlayerReady(this);
                    await SendMessageAsync("READY_OK");
                    break;

                default:
                    Console.WriteLine("[DEBUG] Unknown message received.");
                    await SendMessageAsync("UNKNOWN_COMMAND");
                    break;
            }
        }

        // Implement SetPlayerName to assign a name to the player
        public void SetPlayerName(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
                throw new ArgumentException("Player name cannot be null or empty.", nameof(playerName));

            Name = playerName;
            Console.WriteLine($"[DEBUG] Player name set to: {Name}");
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message + "\n");
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                Console.WriteLine($"[DEBUG] Sent response: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error sending message: {ex.Message}");
            }
        }
    }
}
