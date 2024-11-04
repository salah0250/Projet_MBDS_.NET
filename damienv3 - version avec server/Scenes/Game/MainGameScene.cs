using Godot;
using System;

public partial class MainGameScene : Node2D
{
	private const int UI_PANEL_WIDTH = 200;
	private const int MIN_CELL_SIZE = 50;

	private GridContainer gameGrid;
	private Label resultLabel;
	private Panel[,] cells;
	private bool[,] clickedCells;
	private Button restartButton;
	private Button readyButton;
private bool isTargetSelected;
	private int gridSize = 4;
	private Vector2 targetCell;
	private bool isGameStarted;
	private bool isGamemaster;
	private Label timerLabel;
	private Timer timer;
	private float startTime;

	public override async void _Ready()
	{
		isTargetSelected = false;
		gameGrid = GetNode<GridContainer>("GameGrid");
		resultLabel = GetNode<Label>("ResultLabel");
		timerLabel = GetNode<Label>("TimerLabel");
		timer = GetNode<Timer>("Timer");
		restartButton = GetNode<Button>("RestartButton");
		readyButton = GetNode<Button>("ReadyButton");

		// Configuration des événements
		restartButton.Pressed += OnRestartButtonPressed;
		readyButton.Pressed += OnReadyButtonPressed;
		timer.Timeout += OnTimerTimeout;

		// Configuration de la grille
		SetupGrid(gridSize, gridSize);
		
		// Configuration des événements réseau
		GameNetworkManager.Instance.OnGamemasterAssigned += OnGamemasterAssigned;
		GameNetworkManager.Instance.OnCellSelected += OnRemoteCellSelected;
		GameNetworkManager.Instance.OnGameResults += OnGameResults;
		GameNetworkManager.Instance.OnGameStarted += OnGameStarted;

		try
		{
			await GameNetworkManager.Instance.ConnectToGameServer();
		}
		catch (Exception ex)
		{
			resultLabel.Text = "Erreur de connexion au serveur";
			GD.PrintErr($"Erreur: {ex.Message}");
		}
	}

	private async void OnReadyButtonPressed()
	{
		readyButton.Disabled = true;
		resultLabel.Text = "En attente des autres joueurs...";
		await GameNetworkManager.Instance.SendReadyState();
	}

	private void OnGamemasterAssigned(string gamemaster)
	{
		isGamemaster = gamemaster == SessionManager.Instance.CurrentUsername;
		resultLabel.Text = isGamemaster ? 
			"Vous êtes le maître du jeu ! Sélectionnez une case." :
			"En attente de la sélection du maître du jeu...";
	}

	private void OnGameStarted()
	{
		isGameStarted = true;
		if (!isGamemaster)
		{
			timer.Start();
			startTime = (float)Time.GetTicksMsec() / 1000.0f;
		}
	}

	private void OnRemoteCellSelected(Vector2 position)
	{
		if (!isGamemaster)
		{
			targetCell = position;
			var targetPanel = cells[(int)position.X, (int)position.Y];
			HighlightTargetCell(targetPanel);
			resultLabel.Text = "Case sélectionnée ! Cliquez dessus rapidement !";
		}
	}

	private void OnGameResults(PlayerRanking[] rankings)
	{
		string resultText = "Résultats finaux:\n";
		foreach (var ranking in rankings)
		{
			resultText += $"{ranking.Position}. {ranking.PlayerName}\n";
		}
		resultLabel.Text = resultText;
		isGameStarted = false;
		timer.Stop();
	}

	

	private void HighlightTargetCell(Panel cell)
	{
		var targetStyleBox = new StyleBoxFlat
		{
			BgColor = Colors.Red,
			BorderWidthBottom = 2,
			BorderWidthLeft = 2,
			BorderWidthRight = 2,
			BorderWidthTop = 2,
			BorderColor = new Color(0.2f, 0.2f, 0.2f),
			CornerRadiusBottomLeft = 5,
			CornerRadiusBottomRight = 5,
			CornerRadiusTopLeft = 5,
			CornerRadiusTopRight = 5
		};
		cell.AddThemeStyleboxOverride("panel", targetStyleBox);
	}
	private void OnRestartButtonPressed()
	{
		// Réinitialiser tous les états
		timer.Stop();
		isTargetSelected = false;

		// Réinitialiser les couleurs de toutes les cases
		for (int y = 0; y < gridSize; y++)
		{
			for (int x = 0; x < gridSize; x++)
			{
				var styleBox = new StyleBoxFlat
				{
					BgColor = (x + y) % 2 == 0 ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.6f, 0.6f, 0.6f),
					BorderWidthBottom = 2,
					BorderWidthLeft = 2,
					BorderWidthRight = 2,
					BorderWidthTop = 2,
					BorderColor = new Color(0.2f, 0.2f, 0.2f),
					CornerRadiusBottomLeft = 5,
					CornerRadiusBottomRight = 5,
					CornerRadiusTopLeft = 5,
					CornerRadiusTopRight = 5
				};
				cells[x, y].AddThemeStyleboxOverride("panel", styleBox);
				clickedCells[x, y] = false;
			}
		}

		// Réinitialiser les labels
		resultLabel.Text = "Jeu relancé !";
		timerLabel.Text = "En attente...";

		// Sélectionner une nouvelle case cible
		SelectRandomTargetCell();
	}

	private void SelectRandomTargetCell()
	{
		var random = new Random();
		int targetX = random.Next(0, gridSize);
		int targetY = random.Next(0, gridSize);
		targetCell = new Vector2(targetX, targetY);
		isTargetSelected = true;

		resultLabel.Text = $"MJ a choisi la case ({targetX}, {targetY}). Cliquez dessus !";

		if (cells != null && cells[targetX, targetY] != null)
		{
			var targetPanel = cells[targetX, targetY];
			var targetStyleBox = new StyleBoxFlat
			{
				BgColor = Colors.Red,
				BorderWidthBottom = 2,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderColor = new Color(0.2f, 0.2f, 0.2f),
				CornerRadiusBottomLeft = 5,
				CornerRadiusBottomRight = 5,
				CornerRadiusTopLeft = 5,
				CornerRadiusTopRight = 5
			};
			targetPanel.AddThemeStyleboxOverride("panel", targetStyleBox);

			startTime = (float)Time.GetTicksMsec() / 1000.0f;
			timer.WaitTime = 5.0f;
			timer.Start();
		}
	}

	public void SetupGrid(int width, int height)
	{
		foreach (var child in gameGrid.GetChildren())
		{
			child.QueueFree();
		}
		gameGrid.Columns = width;
		cells = new Panel[width, height];
		clickedCells = new bool[width, height];

		var viewportSize = GetViewport().GetVisibleRect().Size;
		var availableWidth = viewportSize.X - (2 * UI_PANEL_WIDTH) - 80;
		var availableHeight = viewportSize.Y - 40;
		var cellSize = Mathf.Max(MIN_CELL_SIZE, Mathf.Min(
			Mathf.FloorToInt(availableWidth / width),
			Mathf.FloorToInt(availableHeight / height)
		));

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				var cell = CreateCell(x, y, cellSize);
				gameGrid.AddChild(cell);
				cells[x, y] = cell;
				clickedCells[x, y] = false;
			}
		}
	}

	private Panel CreateCell(int x, int y, int size)
	{
		var cell = new Panel
		{
			CustomMinimumSize = new Vector2(size, size)
		};

		var styleBox = new StyleBoxFlat
		{
			
			BgColor = (x + y) % 2 == 0 ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.6f, 0.6f, 0.6f),
			BorderWidthBottom = 2,
			BorderWidthLeft = 2,
			BorderWidthRight = 2,
			BorderWidthTop = 2,
			BorderColor = new Color(0.2f, 0.2f, 0.2f),
			CornerRadiusBottomLeft = 5,
			CornerRadiusBottomRight = 5,
			CornerRadiusTopLeft = 5,
			CornerRadiusTopRight = 5
		};
		cell.AddThemeStyleboxOverride("panel", styleBox);

		cell.GuiInput += (InputEvent @event) =>
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
			{
				if (mouseButton.ButtonIndex == MouseButton.Left)
				{
					OnTileClicked(new Vector2(x, y));
				}
			}
		};

		return cell;
	}

	  private async void OnTileClicked(Vector2 position)
	{
		if (isGamemaster && !isGameStarted)
		{
			await GameNetworkManager.Instance.SendCellSelection(position);
			return;
		}

		if (!isGameStarted || timer.TimeLeft <= 0 || isGamemaster)
			return;

		int x = (int)position.X;
		int y = (int)position.Y;

		if (!clickedCells[x, y])
		{
			clickedCells[x, y] = true;

			var clickedStyleBox = new StyleBoxFlat
			{
				BgColor = Colors.Green,
				BorderWidthBottom = 2,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthTop = 2,
				BorderColor = new Color(0.2f, 0.2f, 0.2f),
				CornerRadiusBottomLeft = 5,
				CornerRadiusBottomRight = 5,
				CornerRadiusTopLeft = 5,
				CornerRadiusTopRight = 5
			};
			cells[x, y].AddThemeStyleboxOverride("panel", clickedStyleBox);

			if (position == targetCell)
			{
				timer.Stop();
				float endTime = (float)Time.GetTicksMsec() / 1000.0f;
				float responseTime = endTime - startTime;
				resultLabel.Text = $"Bravo ! Votre temps de réaction : {responseTime:F2} secondes";
				await GameNetworkManager.Instance.SendGameResult(responseTime);
			}
		}
	}

	public override void _ExitTree()
	{
		GameNetworkManager.Instance.Disconnect();
		base._ExitTree();
	}

	public override void _Process(double delta)
	{
		if (timer != null && isTargetSelected)
		{
			float timeLeft = (float)timer.TimeLeft;
			timerLabel.Text = $"Temps restant : {timeLeft:F2} s";

			if (timeLeft <= 0)
			{
				timer.Stop();
				resultLabel.Text = "Temps écoulé ! Game Over";
				timerLabel.Text = "Jeu terminé";
				isTargetSelected = false;
			}
		}
	}

	private void OnTimerTimeout()
	{
		if (isTargetSelected)
		{
			resultLabel.Text = "Temps écoulé ! Game Over";
			timerLabel.Text = "Jeu terminé";
			isTargetSelected = false;
		}
	}

	public void UpdateGameState(string gameState)
	{
		// TODO: Mettre à jour l'état du jeu en fonction des données reçues du serveur
	}
}
