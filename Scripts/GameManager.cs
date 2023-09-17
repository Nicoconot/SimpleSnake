using Godot;
using System;
using SimpleSnake;

public partial class GameManager : Node
{
	public static GameManager instance;
	public static GameState gameState = GameState.Menu;

	public static Action OnGameStarted, OnGameOver, OnScoreUpdated;

	public static int score = 1;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		instance = this;
	}

	public void StartGame()
	{
		//Start game
		ResetScore();
		gameState = GameState.Playing;
		
		OnGameStarted?.Invoke();
		
		GD.Print("Game Started");
	}

	public void GameOver()
	{
		GD.Print("Game Over");
		gameState = GameState.GameOver;
		OnGameOver?.Invoke();
	}

	public void AddScore()
	{
		score += 1;
		OnScoreUpdated?.Invoke();
	}

	private void ResetScore()
	{
		score = 1;
		OnScoreUpdated?.Invoke();
	}
	
}
