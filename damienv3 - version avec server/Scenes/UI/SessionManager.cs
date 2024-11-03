using Godot;
using System;
using System.Threading.Tasks;

public partial class SessionManager : Node
{
	public static SessionManager Instance { get; private set; }
	
	public bool IsLoggedIn => !string.IsNullOrEmpty(AuthenticationService.CurrentUsername);
	
	private const float TOKEN_VALIDATION_INTERVAL = 60.0f;
	private float tokenValidationTimer = 0;
	
	[Signal]
	public delegate void SessionStateChangedEventHandler(bool isLoggedIn);
	
	public override void _EnterTree()
	{
		if (Instance == null)
		{
			Instance = this;
			ProcessMode = ProcessModeEnum.Always;
		}
		else
		{
			QueueFree();
		}
	}
	
	public override async void _Ready()
	{
		await InitializeNetworkConnection();
		await RestoreSessionAsync();
	}
	
	public override void _Process(double delta)
	{
		if (IsLoggedIn)
		{
			tokenValidationTimer += (float)delta;
			if (tokenValidationTimer >= TOKEN_VALIDATION_INTERVAL)
			{
				tokenValidationTimer = 0;
				_ = ValidateSessionAsync();
			}
		}
	}

	private async Task InitializeNetworkConnection()
	{
		bool connected = await NetworkService.ConnectToServer();
		if (!connected)
		{
			GD.PrintErr("Échec de la connexion au serveur de jeu");
			// Vous pouvez ajouter ici une logique de retry ou d'affichage d'erreur
		}
	}
	
	private async Task RestoreSessionAsync()
	{
		var savedUsername = LoadSavedUsername();
		
		if (!string.IsNullOrEmpty(savedUsername))
		{
			AuthenticationService.RestoreSession(savedUsername);
			if (await AuthenticationService.ValidateTokenAsync())
			{
				EmitSignal(SignalName.SessionStateChanged, true);
				GD.Print("Session restored successfully");
			}
			else
			{
				AuthenticationService.Logout();
				ClearSavedCredentials();
				EmitSignal(SignalName.SessionStateChanged, false);
			}
		}
	}
	
	private async Task ValidateSessionAsync()
	{
		if (!await AuthenticationService.ValidateTokenAsync())
		{
			AuthenticationService.Logout();
			ClearSavedCredentials();
			EmitSignal(SignalName.SessionStateChanged, false);
			
			GetTree().ChangeSceneToFile("res://Scenes/UI/LoginScreen.tscn");
		}
	}
	
	public void SaveSession(string username)
	{
		var config = new ConfigFile();
		config.SetValue("Session", "Username", username);
		config.Save("user://session.cfg");
	}
	
	private string LoadSavedUsername()
	{
		var config = new ConfigFile();
		Error err = config.Load("user://session.cfg");
		if (err == Error.Ok)
		{
			return (string)config.GetValue("Session", "Username", "");
		}
		return "";
	}
	
	public void ClearSavedCredentials()
	{
		var dir = DirAccess.Open("user://");
		if (dir != null)
		{
			dir.Remove("session.cfg");
		}
	}
	
	public async Task<bool> LogoutAsync()
	{
		try
		{
			await NetworkService.SendMessageAsync(new { Type = "Logout" });
			AuthenticationService.Logout();
			ClearSavedCredentials();
			EmitSignal(SignalName.SessionStateChanged, false);
			NetworkService.Disconnect();
			return true;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Erreur lors de la déconnexion: {ex.Message}");
			return false;
		}
	}
	public string CurrentUsername 
	{
		get => AuthenticationService.CurrentUsername ?? "";
		set 
		{
			if (AuthenticationService.CurrentUsername != value)
			{
				// Mettre à jour le nom d'utilisateur dans le service d'authentification
				// via une méthode dédiée
				AuthenticationService.UpdateCurrentUsername(value);
				
				// Sauvegarder la session avec le nouveau nom
				SaveSession(value);
				
				// Émettre le signal de changement d'état si nécessaire
				EmitSignal(SignalName.SessionStateChanged, !string.IsNullOrEmpty(value));
			}
		}
	}
}
