using Godot;
using System.Threading.Tasks;
using System;

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
		
		// Connect to the NetworkManager's MessageReceived signal
		networkManager.MessageReceived += OnNetworkMessageReceived;
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

			// Create and send the login command
			var command = new Command("LOGIN");
			command.Data["username"] = username;
			command.Data["password"] = password;

			byte[] message = SerializationUtils.SerializeMessage(command);
			await networkManager.SendMessageAsync(message);

			// Wait for server response with a timeout
			using var cts = new System.Threading.CancellationTokenSource(5000); // 5 second timeout
			
			bool loginSuccess = await loginResponseReceived.Task.WaitAsync(cts.Token);
			
			if (loginSuccess)
			{
				GD.Print("[DEBUG] Login successful, changing scene...");
				GetTree().ChangeSceneToFile("res://Scenes/UI/NameSelectionScreen.tscn");
			}
			else
			{
				GD.Print("[DEBUG] Login failed, showing error message");
				ShowErrorMessage("Nom d'utilisateur ou mot de passe incorrect.");
			}
		}
		catch (OperationCanceledException)
		{
			ShowErrorMessage("Le serveur ne répond pas. Veuillez réessayer plus tard.");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[DEBUG] Login error: {ex.Message}");
			ShowErrorMessage($"Une erreur est survenue: {ex.Message}");
		}
		finally
		{
			loginButton.Disabled = false;
		}
	}

	private void OnNetworkMessageReceived(byte[] data)
	{
		try
		{
			Command response = SerializationUtils.DeserializeMessage(data);
			GD.Print($"[DEBUG] Received message type: {response.Type}");

			switch (response.Type)
			{
				case "AUTH_SUCCESS":
					GD.Print("[DEBUG] Authentication successful");
					loginResponseReceived?.TrySetResult(true);
					break;

				case "AUTH_FAILED":
					GD.Print("[DEBUG] Authentication failed");
					loginResponseReceived?.TrySetResult(false);
					break;

				default:
					GD.Print($"[DEBUG] Unexpected message type: {response.Type}");
					break;
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[DEBUG] Error processing message: {ex.Message}");
			loginResponseReceived?.TrySetException(ex);
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
		if (networkManager != null)
		{
			networkManager.MessageReceived -= OnNetworkMessageReceived;
		}
		loginResponseReceived?.TrySetCanceled();
		base._ExitTree();
	}
}
