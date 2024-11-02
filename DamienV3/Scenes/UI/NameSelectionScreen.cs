using Godot;
using System;
using System.Threading.Tasks;

public partial class NameSelectionScreen : Control
{
	private LineEdit nameLineEdit;
	private Button confirmButton;
	private NetworkManager networkManager;
	private TaskCompletionSource<bool> nameValidationResponse;

	public override void _Ready()
	{
		nameLineEdit = GetNode<LineEdit>("NameLineEdit");
		confirmButton = GetNode<Button>("ConfirmButton");

		// Access the NetworkManager (Singleton) for network communication
		networkManager = GetNode<NetworkManager>("/root/NetworkManager");

		// Connect the confirm button
		confirmButton.Connect("pressed", new Callable(this, nameof(OnConfirmButtonPressed)));

		// Connect to the NetworkManager's MessageReceived signal
		networkManager.Connect("MessageReceived", new Callable(this, nameof(OnNetworkMessageReceived)));
	}

	private async void OnConfirmButtonPressed()
	{
		string playerName = nameLineEdit.Text.Trim();

		if (!string.IsNullOrEmpty(playerName))
		{
			// Initialize TaskCompletionSource to wait for a response
			nameValidationResponse = new TaskCompletionSource<bool>();

			// Construct a plain text message for name validation
			string nameMessage = $"NAME {playerName}";
			await networkManager.SendMessageAsync(nameMessage);

			// Await server response on name validation
			bool nameIsValid = await nameValidationResponse.Task;

			if (nameIsValid)
			{
				// Name is valid, proceed to the next screen
				GetTree().ChangeSceneToFile("res://Scenes/UI/ReadyCheckScreen.tscn");
			}
			else
			{
				// Display an error message if the name is invalid
				ShowNameError("Le nom choisi est invalide ou déjà utilisé. Veuillez choisir un autre nom.");
			}
		}
		else
		{
			// Display an error if the name field is empty
			ShowNameError("Veuillez entrer un nom pour votre joueur.");
		}
	}

	private void OnNetworkMessageReceived(string message)
	{
		// Check if the message received is related to name validation
		if (message == "NAME_OK")
		{
			nameValidationResponse?.TrySetResult(true);
		}
		else if (message == "NAME_TAKEN")
		{
			nameValidationResponse?.TrySetResult(false);
		}
	}

	private void ShowNameError(string message)
	{
		var errorDialog = new AcceptDialog();
		errorDialog.DialogText = message;
		errorDialog.Title = "Erreur de sélection du nom";
		AddChild(errorDialog);
		errorDialog.PopupCentered();
	}
}
