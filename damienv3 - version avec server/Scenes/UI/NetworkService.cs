using Godot;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;

public static class NetworkService
{
	private static TcpClient client;
	private static NetworkStream stream;
	private static readonly string SERVER_IP = "127.0.0.1"; // À modifier selon votre configuration
	private static readonly int SERVER_PORT = 5000; // À modifier selon votre configuration

	public static bool IsConnected => client?.Connected ?? false;

	public static async Task<bool> ConnectToServer(string ipAddress = null, int? port = null)
	{
		try
		{
			string serverIp = ipAddress ?? SERVER_IP;
			int serverPort = port ?? SERVER_PORT;

			client = new TcpClient();
			await client.ConnectAsync(serverIp, serverPort);
			stream = client.GetStream();
			GD.Print($"Connecté au serveur: {serverIp}:{serverPort}");
			return true;
		}
		catch (Exception ex)
		{
			GD.PrintErr("Erreur de connexion au serveur: ", ex.Message);
			return false;
		}
	}

	public static async Task<bool> ReconnectIfNeeded()
	{
		if (!IsConnected)
		{
			return await ConnectToServer();
		}
		return true;
	}

	public static async Task SendMessageAsync(object message)
{
	try
	{
		if (!IsConnected)
		{
			if (!await ReconnectIfNeeded())
			{
				throw new Exception("Non connecté au serveur");
			}
		}

		if (stream != null)
		{
			// Logging détaillé
			GD.Print($"=== Envoi du message ===");
			if (message is GameMessage gameMessage)
			{
				GD.Print($"Type: {gameMessage.Type}");
				GD.Print($"Gamemaster: {gameMessage.Gamemaster}");
				GD.Print($"Rankings Count: {gameMessage.Rankings?.Count ?? 0}");
			}

			byte[] data = MessagePackSerializer.Serialize(message);
			GD.Print($"Données sérialisées (bytes): {BitConverter.ToString(data)}");
			await stream.WriteAsync(data, 0, data.Length);
			await stream.FlushAsync();
		}
	}
	catch (Exception ex)
	{
		GD.PrintErr($"Erreur d'envoi: {ex.Message}");
		GD.PrintErr($"StackTrace: {ex.StackTrace}");
		throw;
	}
}
public static async Task<T> ReceiveMessageAsync<T>()
{
	try
	{
		if (!IsConnected)
		{
			if (!await ReconnectIfNeeded())
			{
				throw new Exception("Non connecté au serveur");
			}
		}
		if (stream != null)
		{
			byte[] buffer = new byte[8192];
			int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
			if (bytesRead > 0)
			{
				var memory = buffer.AsMemory(0, bytesRead);

				// Journaliser les données reçues avant la désérialisation
				GD.Print("Données reçues (byte array) :", BitConverter.ToString(memory.ToArray()));

				// Utiliser le type générique T pour la désérialisation
				T message = MessagePackSerializer.Deserialize<T>(memory);

				// Journaliser le message après la désérialisation
				GD.Print("Message désérialisé :", message);

				return message;
			}
		}
		throw new Exception("Aucune donnée reçue");
	}
	catch (Exception ex)
	{
		GD.PrintErr($"Erreur de réception: {ex.Message}");
		throw;
	}
}

public static async Task<NameValidationResponse> ReceiveNameValidationResponseAsyncs()
{
	try
	{
		if (!IsConnected)
		{
			if (!await ReconnectIfNeeded())
			{
				throw new Exception("Non connecté au serveur");
			}
		}
		if (stream != null)
		{
			byte[] buffer = new byte[8192];
			int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
			if (bytesRead > 0)
			{
				var memory = buffer.AsMemory(0, bytesRead);

				// Journaliser les données reçues avant la désérialisation
				GD.Print("Données reçues (byte array) :", BitConverter.ToString(memory.ToArray()));

				// Désérialiser spécifiquement en NameValidationResponse
				NameValidationResponse message = MessagePackSerializer.Deserialize<NameValidationResponse>(memory);

				// Journaliser le message après la désérialisation
				GD.Print("Message désérialisé :", message);

				return message;
			}
		}
		throw new Exception("Aucune donnée reçue");
	}
	catch (Exception ex)
	{
		GD.PrintErr($"Erreur de réception: {ex.Message}");
		throw;
	}
}

	
	public static void Disconnect()
	{
		try
		{
			stream?.Close();
			client?.Close();
			stream = null;
			client = null;
			GD.Print("Déconnecté du serveur.");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Erreur lors de la déconnexion: {ex.Message}");
		}
	}
}



[MessagePackObject]
public class SaveNameResponse
{
	[Key(0)]
	public bool Success { get; set; }
}
