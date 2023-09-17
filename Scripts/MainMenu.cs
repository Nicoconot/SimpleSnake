using Godot;
using System;

public partial class MainMenu : Node
{
	[Export] private Control mainMenuParent;
	[Export] private Control gameOverMenuParent;

	[Export] private Button startGameButton, restartButton;
	[Export] private Label scoreText;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GameManager.OnGameOver += ShowGameOverScreen;
		GameManager.OnScoreUpdated += UpdateScoreText;

		startGameButton.Pressed += HideMenus;
		restartButton.Pressed += ShowMenu;
	}

	private void HideMenus()
	{
		GameManager.instance.StartGame();
		gameOverMenuParent.Visible = false;
		mainMenuParent.Visible = false;
	}

	private void ShowMenu()
	{
		gameOverMenuParent.Visible = false;
		mainMenuParent.Visible = true;
	}

	private void ShowGameOverScreen()
	{
		gameOverMenuParent.Visible = true;
		mainMenuParent.Visible = false;
	}

	private void UpdateScoreText()
	{
		scoreText.Text = "Score: " + GameManager.score;
	}
	
}
