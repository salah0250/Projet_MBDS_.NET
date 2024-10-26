using Godot;
using System;

public partial class NameSelectionScreen : Control
{
	private LineEdit nameLineEdit;
	private Button confirmButton;

	public override void _Ready()
	{
		nameLineEdit = GetNode<LineEdit>("NameLineEdit");
		confirmButton = GetNode<Button>("ConfirmButton");

		confirmButton.Connect("pressed", new Callable(this, nameof(OnConfirmButtonPressed)));
	}

	private void OnConfirmButtonPressed()
	{
		string playerName = nameLineEdit.Text.Trim();

		if (!string.IsNullOrEmpty(playerName))
		{
			// TODO: Envoyer le nom du joueur au serveur pour validation

			if (IsNameValid(playerName))
			{
				// Nom valide, naviguer vers l'écran de préparation
				GetTree().ChangeSceneToFile("res://Scenes/UI/ReadyCheckScreen.tscn");
			}
			else
			{
				// Afficher un message d'erreur si le nom est invalide
				ShowNameError("Le nom choisi est invalide ou déjà utilisé. Veuillez choisir un autre nom.");
			}
		}
		else
		{
			// Afficher un message d'erreur si le champ du nom est vide
			ShowNameError("Veuillez entrer un nom pour votre joueur.");
		}
	}

	private bool IsNameValid(string playerName)
	{
		// TODO: Implémenter la logique de validation du nom avec le serveur
		// Pour l'instant, cette fonction retourne toujours vrai pour les besoins de l'exemple
		return true;
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
