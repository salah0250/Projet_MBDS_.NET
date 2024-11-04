using Godot;
using System;
using System.Threading.Tasks;
using System.Linq;
using MessagePack;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

public class GameNetworkManager
{
	private static GameNetworkManager instance;
	public static GameNetworkManager Instance
	{
		get
		{
			instance ??= new GameNetworkManager();
			return instance;
		}
	}

	public event Action<Godot.Vector2> OnCellSelected;
	public event Action<string> OnGamemasterAssigned;
	public event Action OnGameStarted;
	public event Action<PlayerRanking[]> OnGameResults;
	public event Action OnReadyStateChanged;

	private bool isGamemaster;
	private bool isConnected;

	public bool IsConnected => isConnected;

	public async Task ConnectToGameServer()
	{
		if (!isConnected)
		{
			// Préparer les données d'authentification
			var authData = new AuthenticationData
			{
				PlayerName = SessionManager.Instance.CurrentUsername,
				Token = await GetAuthToken() // Nouvelle méthode pour obtenir le token
			};

			try
			{
				await NetworkService.ConnectToServer();
				await NetworkService.SendMessageAsync(authData);
			   	var response = await NetworkService.ReceiveNameValidationResponseAsyncs();
				isConnected = true;

				// Démarrer la boucle d'écoute des messages
				_ = StartMessageListening();
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Erreur de connexion au serveur de jeu: {ex.Message}");
				throw;
			}
		}
	}

	private async Task<string> GetAuthToken()
	{
		// Récupérer le token depuis le cookie ou autre mécanisme d'authentification
		return await Task.FromResult(AuthenticationService.GetCookieValue(".AspNetCore.Identity.Application") ?? string.Empty);
	}

	private async Task StartMessageListening()
{
	while (isConnected)
	{
		try
		{
			var message = await NetworkService.ReceiveMessageAsync<GameMessage>();
			Console.WriteLine($"Message reçu : {message.Type}");
			await ProcessGameMessage(message);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Erreur lors de la réception du message: {ex.Message}");
			isConnected = false;
			break;
		}
	}
}

	private Dictionary<string, bool> playerReadyStates = new Dictionary<string, bool>();

	private async Task ProcessGameMessage(GameMessage message)
	{
		switch (message.Type)
		{
			case GameMessageType.Ready:
				playerReadyStates[message.Gamemaster] = true;
				Console.WriteLine($"{message.Gamemaster} is ready.");

				// Vérifiez si tous les joueurs sont prêts
				if (playerReadyStates.Values.All(ready => ready))
				{
					Console.WriteLine("All players are ready. Starting the game...");
					await StartGame();
				}
				break;
			case GameMessageType.GameStart:
				isGamemaster = message.Gamemaster == SessionManager.Instance.CurrentUsername;
				OnGamemasterAssigned?.Invoke(message.Gamemaster);
				OnGameStarted?.Invoke();
				break;
			case GameMessageType.CellSelected:
				if (!isGamemaster)
				{
					OnCellSelected?.Invoke(message.CellPosition.ToGodotVector());
				}
				break;
			case GameMessageType.GameResults:
				OnGameResults?.Invoke(message.Rankings.ToArray());
				break;
		}
	}

	private async Task StartGame()
	{
		var message = new GameMessage
		{
			Type = GameMessageType.GameStart,
			Gamemaster = SessionManager.Instance.CurrentUsername
		};
		await NetworkService.SendMessageAsync(message);
	}

	public async Task SendGameResult(float reactionTime)
	{
		var message = new GameMessage
		{
			Type = GameMessageType.GameResults,
			Rankings = new List<PlayerRanking>
			{
				new PlayerRanking
				{
					PlayerName = SessionManager.Instance.CurrentUsername,
				}
			}
		};
		await NetworkService.SendMessageAsync(message);
	}

	public async Task SendReadyState()
{
	var message = new GameMessage
	{
		Type = GameMessageType.Ready,
		Gamemaster = SessionManager.Instance.CurrentUsername,
		CellPosition = new NetworkVector2(0, 0),
		PlayersList = new List<string>(),
		Rankings = new List<PlayerRanking>()
	};

	
	// Affichage détaillé du contenu du message
	GD.Print("=== Envoi du message Ready ===");
	GD.Print($"Type: {message.Type}");
	GD.Print($"Gamemaster: {message.Gamemaster}");
	GD.Print($"CellPosition: X={message.CellPosition.X}, Y={message.CellPosition.Y}");
	GD.Print("PlayersList: [" + string.Join(", ", message.PlayersList) + "]");
	GD.Print("Rankings: [");
	foreach (var rank in message.Rankings)
	{
		GD.Print($"  Player: {rank.PlayerName}, Position: {rank.Position}, IsEligible: {rank.IsEligible}");
	}
	GD.Print("]");
	GD.Print("==========================");
	
	try 
	{
		await NetworkService.SendMessageAsync(message);
		GD.Print("Message Ready envoyé avec succès");
		_ = StartMessageListening();
	}
	catch (Exception ex)
	{
		GD.PrintErr($"Erreur lors de l'envoi du message Ready: {ex.Message}");
		throw;
	}
	
	OnReadyStateChanged?.Invoke();
}

	public async Task SendCellSelection(Godot.Vector2 position)
	{
		if (!isGamemaster) return;

		var message = new GameMessage
		{
			Type = GameMessageType.CellSelected,
			// Convertir Godot.Vector2 en NetworkVector2 pour l'envoi
			CellPosition = NetworkVector2.FromGodotVector(position)
		};
		await NetworkService.SendMessageAsync(message);
	}

	public static bool IsEligible(int pos, string name)
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

	public void Disconnect()
	{
		isConnected = false;
		NetworkService.Disconnect();
	}
}
