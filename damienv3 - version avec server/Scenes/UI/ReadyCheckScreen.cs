using Godot;
using System;
using System.Threading.Tasks;

public partial class ReadyCheckScreen : Control
{
	private Button readyButton;
	private Label waitingLabel;
	private bool isProcessing;

	public override async void _Ready()
{
	
	// Initialisation des contrôles UI
	readyButton = GetNode<Button>("ReadyButton");
	waitingLabel = GetNode<Label>("WaitingLabel");
	
	// Configuration initiale
	if (waitingLabel != null)
	{
		waitingLabel.Visible = false;
	}
	
	// Connexion des événements
	readyButton.Pressed += OnReadyButtonPressed;
	
	// Écouter les événements du GameNetworkManager
	GameNetworkManager.Instance.OnGameStarted += () =>
	{
		// Transition vers l'écran de jeu
		GetTree().ChangeSceneToFile("res://Scenes/Game/MainGameScene.tscn");
	};
}
private async void OnReadyButtonPressed()
{
	if (isProcessing) return;

	try
	{
		isProcessing = true;
		readyButton.Disabled = true;

		// Assurez-vous que le client est connecté au serveur
		

		// Utiliser le GameNetworkManager au lieu d'envoyer directement
		await GameNetworkManager.Instance.SendReadyState();

		ShowWaitingMessage("En attente des autres joueurs...");
	}
	catch (Exception ex)
	{
		GD.PrintErr($"Erreur lors de la vérification ready : {ex.Message}");
		ShowWaitingMessage("Une erreur est survenue. Veuillez réessayer.");
		readyButton.Disabled = false;
	}
	finally
	{
		isProcessing = false;
	}
}

	private void ShowWaitingMessage(string message)
	{
		if (waitingLabel == null)
		{
			waitingLabel = new Label
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			AddChild(waitingLabel);
		}
		
		waitingLabel.Text = message;
		waitingLabel.Visible = true;
	}

	public override void _ExitTree()
	{
		if (readyButton != null)
		{
			readyButton.Pressed -= OnReadyButtonPressed;
		}
		base._ExitTree();
	}
}
