using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public partial class MainGameScene : Node2D
{
	private const int UI_PANEL_WIDTH = 200;
	private const int MIN_CELL_SIZE = 50;

	private GridContainer gameGrid;
	private Label resultLabel;
	private Panel[,] cells;
	private bool[,] clickedCells;
	private Label timerLabel;
	private Timer timer;

	private NetworkManager networkManager;

	private int gridSize = 4;
	private Vector2 targetCell;
	private bool isTargetSelected;
	private bool isMaster; // Indicates if the player is the Game Master (MJ)
	private string sessionID; // Unique ID for each player
	private float startTime;

	private TaskCompletionSource<string> serverResponse;

	public override void _Ready()
	{
		InitializeComponents();
		ConnectSignals();
		SetupGrid(gridSize, gridSize);
		RequestRoleFromServer();
	}

	private void InitializeComponents()
	{
		networkManager = GetNode<NetworkManager>("/root/NetworkManager") ?? throw new InvalidOperationException("NetworkManager not found");
		GD.Print("Trying to locate NetworkManager at path /root/NetworkManager");
		gameGrid = GetNode<GridContainer>("GameGrid") ?? throw new InvalidOperationException("GameGrid not found");
		resultLabel = GetNode<Label>("ResultLabel") ?? throw new InvalidOperationException("ResultLabel not found");
		timerLabel = GetNode<Label>("TimerLabel") ?? throw new InvalidOperationException("TimerLabel not found");
		timer = GetNode<Timer>("Timer") ?? throw new InvalidOperationException("Timer not found");
	}

	private void ConnectSignals()
	{
		timer.Timeout += OnTimerTimeout;
		networkManager.Connect("MessageReceived", new Callable(this, nameof(OnNetworkMessageReceived)));
	}

	private bool isRoleAssigned = false; // Flag to track if a role has already been assigned
	private bool roleRequestSent = false; // Flag to ensure ROLE_REQUEST is sent only once

	private async void RequestRoleFromServer()
	{
		if (roleRequestSent || isRoleAssigned) return; // Prevent duplicate requests
		roleRequestSent = true; // Mark that the role request has been sent

		serverResponse = new TaskCompletionSource<string>();

		// Créez un objet Command pour la requête de rôle
		var command = new Command("ROLE_REQUEST");

		// Sérialisez le message avec MessagePack
		byte[] message = SerializationUtils.SerializeMessage(command);
		await networkManager.SendMessageAsync(message);

		GD.Print("Role request sent to server. Waiting for response...");

		try
		{
			string response = await serverResponse.Task;
			ProcessRoleResponse(response);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error in role assignment: {ex.Message}");
			resultLabel.Text = "Erreur: Assignation de role echouee";
		}
	}

	private async void ProcessRoleResponse(string response)
	{
		var parts = response.Split(' ');

		if (parts.Length < 2)
		{
			GD.PrintErr($"Invalid role response format: {response}");
			resultLabel.Text = "Erreur: Réponse de role invalide";
			return;
		}

		if (parts[0] == "ROLE_MJ")
		{
			isMaster = true;
			sessionID = parts[1];
			isRoleAssigned = true;
			GD.Print($"Assigned role: MJ with Session ID: {sessionID}");
			SelectRandomTargetCell();
		}
		else if (parts[0] == "ROLE_PLAYER")
		{
			isMaster = false;
			sessionID = parts[1];
			isRoleAssigned = true;
			resultLabel.Text = "En attente du MJ...";
			GD.Print($"Assigned role: Player with Session ID: {sessionID}");
		}
		else
		{
			GD.PrintErr($"Unknown role response: {response}");
			resultLabel.Text = "Erreur: Reponse de role inconnue";
		}

		if (isRoleAssigned)
		{
			var readyCommand = new Command("READY");
			byte[] readyMessage = SerializationUtils.SerializeMessage(readyCommand);
			await networkManager.SendMessageAsync(readyMessage);
		}
	}

	private async void SelectRandomTargetCell()
	{
		if (!isMaster || isTargetSelected)
			return;

		Random random = new Random();
		int targetX = random.Next(0, gridSize);
		int targetY = random.Next(0, gridSize);
		targetCell = new Vector2(targetX, targetY);
		isTargetSelected = true;

		SetCellColor(targetX, targetY, Colors.Red);

		// Créez et envoyez la commande TARGET avec les coordonnées
		var targetCommand = new Command("TARGET");
		targetCommand.Data["x"] = targetX.ToString();
		targetCommand.Data["y"] = targetY.ToString();
		byte[] targetMessage = SerializationUtils.SerializeMessage(targetCommand);
		await networkManager.SendMessageAsync(targetMessage);

		startTime = (float)Time.GetTicksMsec() / 1000.0f;
		timer.WaitTime = 5.0f;
		timer.Start();
		UpdateTimerLabel((float)timer.WaitTime);
	}


	private void UpdateTimerLabel(float timeLeft)
	{
		timerLabel.Text = $"Temps restant: {timeLeft:F1}s";
	}

	private void OnNetworkMessageReceived(byte[] data)
	{
		var command = SerializationUtils.DeserializeMessage(data);

		switch (command.Type)
		{
			case "ROLE_MJ":
				ProcessRoleResponse(command.Data["response"].ToString());
				break;

			case "TARGET":
				int targetX = int.Parse(command.Data["x"].ToString());
				int targetY = int.Parse(command.Data["y"].ToString());
				HandleTargetMessage(targetX, targetY);
				break;

			case "RESULT":
				DisplayResults(command.Data["results"]);
				break;

			case "START_GAME":
				HandleStartGame();
				break;

			default:
				GD.PrintErr($"Unknown message type: {command.Type}");
				break;
		}
	}

	private void DisplayResults(object resultsData)
	{
		// Assuming resultsData is a list of dictionaries or similar structure
		if (resultsData is IEnumerable<object> resultsList)
		{
			string resultsText = "Resultats:\n";

			foreach (var result in resultsList)
			{
				if (result is Dictionary<string, object> resultDict)
				{
					// Assuming "Name" and "Time" are keys in the result dictionary
					string playerName = resultDict["Name"].ToString();
					string responseTime = resultDict["Time"].ToString();

					resultsText += $"{playerName}: {responseTime} s\n";
				}
			}

			resultLabel.Text = resultsText;
		}
		else
		{
			GD.PrintErr("Invalid results data format.");
		}
	}

	private void HandleStartGame()
	{
		GD.Print("Game started!");
		resultLabel.Text = "Le jeu a commence!";
	}

	private void HandleTargetMessage(int x, int y)
	{
		targetCell = new Vector2(x, y);
		isTargetSelected = true;
		SetCellColor(x, y, Colors.Red);
	}

	private void HandleClick(string[] parts)
	{
		if (parts.Length >= 3 && float.TryParse(parts[2], out float time))
		{
			resultLabel.Text = $"{parts[1]} a gagne ! Temps : {time:F2} s";
			timer.Stop();
		}
	}

	private void SetupGrid(int width, int height)
	{
		gameGrid.Columns = width;
		cells = new Panel[width, height];
		clickedCells = new bool[width, height];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				var cell = CreateCell(x, y);
				gameGrid.AddChild(cell);
				cells[x, y] = cell;
				clickedCells[x, y] = false;
				SetCellColor(x, y, (x + y) % 2 == 0 ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.6f, 0.6f, 0.6f));
			}
		}
	}

	private Panel CreateCell(int x, int y)
	{
		var cell = new Panel { CustomMinimumSize = new Vector2(MIN_CELL_SIZE, MIN_CELL_SIZE) };
		cell.GuiInput += (InputEvent @event) =>
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
			{
				OnTileClicked(new Vector2(x, y));
			}
		};

		return cell;
	}

	private void SetCellColor(int x, int y, Color color)
	{
		var styleBox = new StyleBoxFlat { BgColor = color };
		cells[x, y]?.AddThemeStyleboxOverride("panel", styleBox);
	}

	private async void OnTileClicked(Vector2 position)
	{
		if (!isTargetSelected || timer.TimeLeft <= 0) return;

		int x = (int)position.X;
		int y = (int)position.Y;

		if (position == targetCell)
		{
			float responseTime = ((float)Time.GetTicksMsec() / 1000.0f) - startTime;

			// Créez et envoyez la commande CLICK avec le temps de réponse
			var clickCommand = new Command("CLICK");
			clickCommand.Data["sessionID"] = sessionID;
			clickCommand.Data["responseTime"] = responseTime.ToString(); // Conversion du float en string
			byte[] clickMessage = SerializationUtils.SerializeMessage(clickCommand);
			await networkManager.SendMessageAsync(clickMessage);
		}
	}



	private void OnTimerTimeout()
	{
		if (isTargetSelected)
		{
			resultLabel.Text = "Temps ecoule ! Game Over";
			timerLabel.Text = "Jeu termine";
			isTargetSelected = false;
		}
	}
}
