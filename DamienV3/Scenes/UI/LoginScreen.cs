using Godot;
using System.Threading.Tasks;

public partial class LoginScreen : Control
{
	private LineEdit usernameLineEdit;
	private LineEdit passwordLineEdit;
	private Button loginButton;
	private NetworkManager networkManager;
	private TaskCompletionSource<bool> loginResponseReceived;

	public override void _Ready()
	{
		usernameLineEdit = GetNode<LineEdit>("UsernameLineEdit");
		passwordLineEdit = GetNode<LineEdit>("PasswordLineEdit");
		loginButton = GetNode<Button>("LoginButton");

		// Access the NetworkManager (singleton) for network communication
		networkManager = GetNode<NetworkManager>("/root/NetworkManager");

		// Connect the button
		loginButton.Pressed += OnLoginButtonPressed;

		// Connect to the MessageReceived signal
		networkManager.Connect(nameof(NetworkManager.MessageReceived),
			new Callable(this, nameof(OnNetworkMessageReceived)));

		GD.Print("[DEBUG] Connected to MessageReceived signal in LoginScreen.");
	}

	private async void OnLoginButtonPressed()
	{
		string username = usernameLineEdit.Text;
		string password = passwordLineEdit.Text;

		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			ShowErrorMessage("Veuillez renseigner les champs nom d'utilisateur et mot de passe.");
			return;
		}

		loginButton.Disabled = true;

		try
		{
			loginResponseReceived = new TaskCompletionSource<bool>();

			// Send plain text login request
			string loginRequest = $"LOGIN {username} {password}";
			await networkManager.SendMessageAsync(loginRequest);
			GD.Print("[DEBUG] Login message sent to server.");

			// Wait for server response with a timeout
			using var cts = new System.Threading.CancellationTokenSource(5000); // 5-second timeout
			bool loginSuccess = await loginResponseReceived.Task.WaitAsync(cts.Token);

			if (loginSuccess)
			{
				GD.Print("[DEBUG] Authentication successful. Switching to NameSelectionScreen.");
				GetTree().ChangeSceneToFile("res://Scenes/UI/NameSelectionScreen.tscn");
			}
			else
			{
				ShowErrorMessage("Nom d'utilisateur ou mot de passe incorrect.");
			}
		}
		catch (System.TimeoutException)
		{
			ShowErrorMessage("Le serveur ne répond pas. Veuillez réessayer plus tard.");
		}
		catch (System.Exception ex)
		{
			ShowErrorMessage($"Une erreur est survenue: {ex.Message}");
		}
		finally
		{
			loginButton.Disabled = false;
		}
	}

	private void OnNetworkMessageReceived(string message)
	{
		if (loginResponseReceived == null)
		{
			GD.Print("[DEBUG] loginResponseReceived is null. Exiting message handling.");
			return;
		}

		GD.Print($"[DEBUG] Received response: {message}");

		if (message.Trim() == "AUTH_SUCCESS")
		{
			GD.Print("[DEBUG] AUTH_SUCCESS received. Transitioning to NameSelectionScreen.");
			loginResponseReceived.TrySetResult(true);
		}
		else if (message.Trim() == "AUTH_FAILED")
		{
			GD.Print("[DEBUG] AUTH_FAILED received.");
			loginResponseReceived.TrySetResult(false);
		}
		else
		{
			GD.PrintErr($"[DEBUG] Unknown response received: {message}");
		}
	}

	private void ShowErrorMessage(string message)
	{
		var errorDialog = new AcceptDialog();
		errorDialog.DialogText = message;
		errorDialog.Title = "Erreur de Connexion";
		AddChild(errorDialog);
		errorDialog.PopupCentered();

		errorDialog.Confirmed += () => errorDialog.QueueFree();
		errorDialog.Canceled += () => errorDialog.QueueFree();
	}

	public override void _ExitTree()
	{
		loginResponseReceived?.TrySetCanceled();
		base._ExitTree();
	}
}
