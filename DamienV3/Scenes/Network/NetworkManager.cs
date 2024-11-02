using Godot;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System;

public partial class NetworkManager : Node
{
	[Signal]
	public delegate void MessageReceivedEventHandler(string data);

	private TcpClient client;
	private NetworkStream stream;
	private const string serverIp = "localhost";
	private const int serverPort = 5000;
	private bool isConnected = false;

	public override void _Ready()
	{
		ConnectToServer();
	}

	private async void ConnectToServer()
	{
		try
		{
			client = new TcpClient();
			await client.ConnectAsync(serverIp, serverPort);
			stream = client.GetStream();
			isConnected = true;
			GD.Print("[DEBUG] Connected to server successfully.");

			// Start listening for messages
			_ = ListenForMessages();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[DEBUG] Failed to connect to server: {ex.Message}");
			isConnected = false;
		}
	}

	public async Task SendMessageAsync(string message)
	{
		if (!isConnected || stream == null)
		{
			GD.PrintErr("[DEBUG] Not connected to server. Attempting to reconnect...");
			ConnectToServer();
			return;
		}

		try
		{
			byte[] messageBytes = Encoding.UTF8.GetBytes(message + "\n"); // Adding newline as delimiter
			await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
			await stream.FlushAsync();

			GD.Print($"[DEBUG] Plain text message sent to server: {message}");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[DEBUG] Error sending message: {ex.Message}");
			isConnected = false;
			ConnectToServer();
		}
	}

	private async Task ListenForMessages()
	{
		byte[] buffer = new byte[1024];
		StringBuilder messageBuilder = new StringBuilder();

		while (isConnected)
		{
			try
			{
				int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
				if (bytesRead == 0)
				{
					GD.PrintErr("[DEBUG] Connection closed by server.");
					break;
				}

				messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

				if (messageBuilder.ToString().Contains("\n"))
				{
					string fullMessage = messageBuilder.ToString().Trim();
					messageBuilder.Clear();
					GD.Print($"[DEBUG] Full message received: {fullMessage}");
					EmitSignal(nameof(MessageReceived), fullMessage);
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[DEBUG] Error receiving data: {ex.Message}");
				break;
			}
		}

		GD.Print("[DEBUG] Listening loop ended. Closing connection.");
		CloseConnection();
	}

	private void CloseConnection()
	{
		isConnected = false;
		stream?.Close();
		client?.Close();
		stream = null;
		client = null;
		GD.Print("[DEBUG] Connection closed.");
	}

	public override void _ExitTree()
	{
		CloseConnection();
		base._ExitTree();
	}
}
