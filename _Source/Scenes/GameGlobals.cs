using Godot;
using RTSGame.Units;

public partial class GameGlobals : Node
{
	public static GameGlobals Instance { get; private set; }

	// Define an enum for clarity
	public enum GameMode
	{
		Normal,
		Rogue,
		Continue,
		Debug,
		Tutorial
	}

	// Store the current mode globally
	public GameMode CurrentMode { get; set; } = GameMode.Normal;

	public LevelResource CurrentLevel;

	public override void _Ready()
	{
		// Singleton pattern assignment for C# access
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
			return;
		}
	}
}