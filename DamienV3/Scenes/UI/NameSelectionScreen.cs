using Godot;
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

			// Créez un objet Command pour la validation du nom
			var nameCommand = new Command("NAME");
			nameCommand.Data["playerName"] = playerName;

			// Sérialisez la commande et envoyez-la
			byte[] nameMessage = SerializationUtils.SerializeMessage(nameCommand);
			await networkManager.SendMessageAsync(nameMessage);

			// Attendre la réponse du serveur
			bool nameIsValid = await nameValidationResponse.Task;

			if (nameIsValid)
			{
				// Le nom est valide, passer à l'écran suivant
				GetTree().ChangeSceneToFile("res://Scenes/UI/ReadyCheckScreen.tscn");
			}
			else
			{
				// Affiche un message d'erreur si le nom est invalide
				ShowNameError("Le nom choisi est invalide ou déjà utilisé. Veuillez choisir un autre nom.");
			}
		}
		else
		{
			// Affiche une erreur si le champ du nom est vide
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
		errorDialog.Title = "Erreur de selection du nom";
		AddChild(errorDialog);
		errorDialog.PopupCentered();
	}
}
