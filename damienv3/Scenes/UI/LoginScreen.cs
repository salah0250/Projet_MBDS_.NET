using Godot;
using System;

public partial class LoginScreen : Control
{
	private LineEdit usernameLineEdit;
	private LineEdit passwordLineEdit;
	private Button loginButton;

	public override void _Ready()
	{
		usernameLineEdit = GetNode<LineEdit>("UsernameLineEdit");
		passwordLineEdit = GetNode<LineEdit>("PasswordLineEdit");
		loginButton = GetNode<Button>("LoginButton");

		loginButton.Connect("pressed", new Callable(this, nameof(OnLoginButtonPressed)));
	}

	private void OnLoginButtonPressed()
	{
		string username = usernameLineEdit.Text;
		string password = passwordLineEdit.Text;

		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			ShowNameError("Veuillez renseigner les champs nom d'utilisateur et mot de passe.");
			return;
		}

		// TODO: Vérifier les identifiants de connexion avec le serveur

		if (IsLoginValid(username, password))
		{
			// Connexion réussie, naviguer vers la scène de sélection du nom
			GetTree().ChangeSceneToFile("res://Scenes/UI/NameSelectionScreen.tscn");
		}
		else
		{
			ShowNameError("Nom d'utilisateur ou mot de passe incorrect.");
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

	private bool IsLoginValid(string username, string password)
	{
		// TODO: Implémenter la vérification des identifiants de connexion avec le serveur
		return true; // Placeholder
	}
}
