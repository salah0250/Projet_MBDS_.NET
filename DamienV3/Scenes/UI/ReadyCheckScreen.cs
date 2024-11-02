using Godot;
using System.Threading.Tasks;

public partial class ReadyCheckScreen : Control
{
	private Button readyButton;
	private NetworkManager networkManager;

	public override void _Ready()
	{
		readyButton = GetNode<Button>("ReadyButton");

		// Access the NetworkManager (Singleton) for network communication
		networkManager = GetNode<NetworkManager>("/root/NetworkManager");

		// Connect the "Ready" button to send the ready message
		readyButton.Connect("pressed", new Callable(this, nameof(OnReadyButtonPressed)));

		// Connect to the NetworkManager's MessageReceived signal to listen for server messages
		networkManager.Connect("MessageReceived", new Callable(this, nameof(OnNetworkMessageReceived)));
	}

	private async void OnReadyButtonPressed()
	{
		// Cr�ez un objet Command pour indiquer que le joueur est pr�t
		var readyCommand = new Command("READY");

		// S�rialisez le message avec MessagePack
		byte[] readyMessage = SerializationUtils.SerializeMessage(readyCommand);
		await networkManager.SendMessageAsync(readyMessage);

		// Afficher un message d'attente pour le joueur
		ShowWaitingMessage("En attente des autres joueurs...");
	}

	private void OnNetworkMessageReceived(string message)
	{
		if (message == "ALL_READY" || message == "START_GAME")
		{
			// All players are ready, switch to the main game scene
			GetTree().ChangeSceneToFile("res://Scenes/Game/MainGameScene.tscn");
		}
		else
		{
			// Display any other messages from the server as waiting messages
			ShowWaitingMessage(message);
		}
	}

	private void ShowWaitingMessage(string message)
	{
		// Display a waiting message for the player
		var waitingLabel = GetNode<Label>("WaitingLabel");

		if (waitingLabel == null)
		{
			waitingLabel = new Label
			{
				Name = "WaitingLabel",
				Text = message,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			AddChild(waitingLabel);
		}
		else
		{
			waitingLabel.Text = message;
		}
	}
}
