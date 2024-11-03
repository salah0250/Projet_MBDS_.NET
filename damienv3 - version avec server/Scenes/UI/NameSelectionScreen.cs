using Godot;
using System;
using System.Threading.Tasks;

public partial class NameSelectionScreen : Control
{
	 [Export]
	private NodePath nameInputPath = "NameLineEdit";
	[Export]
	private NodePath confirmButtonPath = "ConfirmButton";

	private LineEdit nameInput;
	private Button confirmButton;
	
	public override void _Ready()
	{
		try
		{
			nameInput = GetNode<LineEdit>(nameInputPath);
			confirmButton = GetNode<Button>(confirmButtonPath);

			if (nameInput == null || confirmButton == null)
			{
				throw new NullReferenceException("Un ou plusieurs nœuds n'ont pas été trouvés dans la scène.");
			}

			// Configurer les événements et l'état initial
			confirmButton.Pressed += OnConfirmButtonPressed;

			// Vérifier la connexion au serveur au démarrage
			_ = InitializeNetworkConnection();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Erreur lors de l'initialisation: {ex.Message}");
		}
	}

	private void ShowError(string message)
	{
		// Au lieu d'utiliser un Label, utilisez GD.PrintErr pour le débogage
		GD.PrintErr($"Erreur: {message}");
	}
	private async Task InitializeNetworkConnection()
	{
		try
		{
			if (!NetworkService.IsConnected)
			{
				bool connected = await NetworkService.ConnectToServer();
				if (!connected)
				{
					ShowError("Impossible de se connecter au serveur. Veuillez réessayer.");
					confirmButton.Disabled = true;
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Erreur de connexion: {ex.Message}");
			ShowError("Erreur de connexion au serveur.");
			confirmButton.Disabled = true;
		}
	}

	private async void OnConfirmButtonPressed()
{
	if (confirmButton == null || nameInput == null)
	{
		GD.PrintErr("Les composants UI ne sont pas initialisés correctement.");
		return;
	}
	
	confirmButton.Disabled = true;
	 try
	{
		string name = nameInput.Text?.Trim() ?? string.Empty;
		
		if (string.IsNullOrEmpty(name))
		{
			ShowError("Le nom ne peut pas être vide");
			return;
		}
		await SaveNameAsync(name);
		// Au lieu d'assigner directement, utiliser le service d'authentification
		AuthenticationService.UpdateCurrentUsername(name);
		await GetTree().CreateTimer(0.5f).ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		GetTree().ChangeSceneToFile("res://Scenes/UI/ReadyCheckScreen.tscn");
	}
	catch (Exception ex)
	{
		GD.PrintErr($"Erreur: {ex.Message}");
		ShowError("Une erreur est survenue. Veuillez réessayer.");
	}
	finally
	{
		if (confirmButton != null)
		{
			confirmButton.Disabled = false;
		}
	}
}
	

	private async Task<bool> IsNameValidAsync(string name)
{
	try
	{
		var request = new AuthenticationData 
		{
			PlayerName = name,
			Token = AuthenticationService.GetCookieValue(".AspNetCore.Identity.Application")
		};
		await NetworkService.SendMessageAsync(request);
		var response = await NetworkService.ReceiveNameValidationResponseAsyncs();

		return response?.IsValid ?? false;
	}
	catch (Exception ex)
	{
		GD.PrintErr($"Erreur lors de la validation du nom: {ex.Message}");
		throw;
	}
}

private async Task SaveNameAsync(string name)
{
	try
	{
		var request = new AuthenticationData 
		{
			PlayerName = name,
			Token = AuthenticationService.GetCookieValue(".AspNetCore.Identity.Application")
		};
		await NetworkService.SendMessageAsync(request);
		var response = await NetworkService.ReceiveNameValidationResponseAsyncs();

		
		if (!response?.IsValid ?? true)
		{
			throw new Exception("Échec de l'enregistrement du nom");
		}
	}
	catch (Exception ex)
	{
		GD.PrintErr($"Erreur lors de l'enregistrement du nom: {ex.Message}");
		throw;
	}
}

	public override void _ExitTree()
	{
		if (confirmButton != null)
		{
			confirmButton.Pressed -= OnConfirmButtonPressed;
		}
		base._ExitTree();
	}
}
