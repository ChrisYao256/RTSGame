using Godot;

public partial class GameGlobals : Node
{
	// Define an enum for clarity
	public enum GameMode
	{
		Normal,
		Continue,
		Debug,
		Tutorial
	}

	// Store the current mode globally
	public GameMode CurrentMode { get; set; } = GameMode.Normal;
}