using Godot;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System;

public partial class NetworkManager : Node
{
	[Signal]
	public delegate void MessageReceivedEventHandler(byte[] data);

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

	public async Task SendMessageAsync(byte[] message)
	{
		if (!isConnected || stream == null)
		{
			GD.PrintErr("[DEBUG] Not connected to server. Attempting to reconnect...");
			ConnectToServer();
			return;
		}

		try
		{
			// Add message length prefix
			int messageLength = message.Length;
			byte[] lengthPrefix = BitConverter.GetBytes(messageLength);

			// First send length
			await stream.WriteAsync(lengthPrefix, 0, 4);
			// Then send message
			await stream.WriteAsync(message, 0, message.Length);
			await stream.FlushAsync();

			GD.Print($"[DEBUG] Message sent to server. Size: {message.Length} bytes");
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
		byte[] lengthBuffer = new byte[4];

		while (isConnected)
		{
			try
			{
				// Read message length first
				int bytesRead = await stream.ReadAsync(lengthBuffer, 0, 4);
				if (bytesRead < 4)
				{
					GD.PrintErr("[DEBUG] Failed to read message length");
					break;
				}

				int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
				byte[] messageBuffer = new byte[messageLength];

				// Read the full message
				bytesRead = await stream.ReadAsync(messageBuffer, 0, messageLength);
				if (bytesRead < messageLength)
				{
					GD.PrintErr("[DEBUG] Incomplete message received");
					break;
				}

				GD.Print($"[DEBUG] Received message of {bytesRead} bytes");

				// Process the message
				try
				{
					Command deserializedMessage = SerializationUtils.DeserializeMessage(messageBuffer);
					GD.Print($"[DEBUG] Deserialized message type: {deserializedMessage.Type}");

					// Emit the signal with the original message bytes
					EmitSignal(SignalName.MessageReceived, messageBuffer);
				}
				catch (Exception ex)
				{
					GD.PrintErr($"[DEBUG] Error deserializing message: {ex.Message}");
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
