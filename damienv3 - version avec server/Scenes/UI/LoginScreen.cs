using Godot;
using System;

public partial class LoginScreen : Control
{
	private LineEdit usernameLineEdit;
	private LineEdit passwordLineEdit;
	private Button loginButton;
	private Label errorLabel;

	public override void _Ready()
	{
		PrintSceneTree(this);
		// Initialisation des contrôles
		usernameLineEdit = GetNode<LineEdit>("UsernameLineEdit");
		passwordLineEdit = GetNode<LineEdit>("PasswordLineEdit");
		loginButton = GetNode<Button>("LoginButton");
		errorLabel = GetNode<Label>("ErrorLabel");

		// Configuration initiale
		errorLabel.Visible = false;
		passwordLineEdit.Secret = true;
		
		// Connexion des signaux
		loginButton.Connect("pressed", new Callable(this, nameof(OnLoginButtonPressed)));
		
		// Vérification de la session existante
		if (SessionManager.Instance.IsLoggedIn)
		{
			GetTree().ChangeSceneToFile("res://Scenes/UI/NameSelectionScreen.tscn");
		}
	}
private void PrintSceneTree(Node node, string indent = "")
{
	GD.Print($"{indent}{node.Name} ({node.GetType()})");
	foreach (Node child in node.GetChildren())
	{
		PrintSceneTree(child, indent + "  ");
	}
}
	private async void OnLoginButtonPressed()
	{
		string username = usernameLineEdit.Text.Trim();
		string password = passwordLineEdit.Text;

		// Validation des champs
		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			ShowError("Veuillez renseigner tous les champs.");
			return;
		}

		// Désactivation du bouton pendant la tentative
		loginButton.Disabled = true;
		
		try 
		{
			// Tentative de connexion
			bool loginSuccess = await AuthenticationService.LoginAsync(username, password);
			
			if (loginSuccess)
			{
				GetTree().ChangeSceneToFile("res://Scenes/UI/NameSelectionScreen.tscn");
			}
			else
			{
				ShowError("Échec de la connexion. Vérifiez vos identifiants.");
				loginButton.Disabled = false;
			}
		}
		catch (Exception ex)
		{
			ShowError($"Une erreur est survenue : {ex.Message}");
			loginButton.Disabled = false;
		}
	}

	private void ShowError(string message)
	{
		errorLabel.Text = message;
		errorLabel.Visible = true;
	}
}
