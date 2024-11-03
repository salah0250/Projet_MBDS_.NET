using Godot;
using System;

public partial class ReadyCheckScreen : Control
{
	private Button readyButton;

	public override void _Ready()
	{
		readyButton = GetNode<Button>("ReadyButton");
		readyButton.Connect("pressed", new Callable(this, nameof(OnReadyButtonPressed)));
	}

	private void OnReadyButtonPressed()
	{
		// TODO: Envoyer l'état de préparation du joueur au serveur

		if (IsReadyCheckSuccessful())
		{
			// Tous les joueurs sont prêts, naviguer vers la scène de jeu principale
			GetTree().ChangeSceneToFile("res://Scenes/Game/MainGameScene.tscn");
		}
		else
		{
			// Afficher un message d'attente jusqu'à ce que tous les joueurs soient prêts
			ShowWaitingMessage("En attente des autres joueurs...");
		}
	}

	private bool IsReadyCheckSuccessful()
	{
		// TODO: Vérifier avec le serveur si tous les joueurs sont prêts
		// Pour l'instant, cette fonction retourne toujours vrai pour les besoins de l'exemple
		return true;
	}

	private void ShowWaitingMessage(string message)
	{
		var waitingLabel = new Label();
		waitingLabel.Text = message;
		waitingLabel.HorizontalAlignment = HorizontalAlignment.Center;
		waitingLabel.VerticalAlignment = VerticalAlignment.Center;
		AddChild(waitingLabel);
	}
}
