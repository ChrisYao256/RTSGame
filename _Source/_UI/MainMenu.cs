using Godot;
using RTSGame.Units;
using System;
using System.Net.Sockets;

public partial class MainMenu : Control
{
	[Export]
	public string BattleScenePath = "res://_Content/_Scenes/BattleScene.tscn";

	[Export]
	public string TDScenePath = "res://_Content/_Scenes/TDScene.tscn";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("HBoxContainer/TutorialButton").Pressed += () => OnStartButtonPressed(GameGlobals.GameMode.Tutorial);

		// Bind the Debug button to pass GameMode.Debug
		GetNode<Button>("HBoxContainer/TestingButton").Pressed += () => OnStartButtonPressed(GameGlobals.GameMode.Debug);

		GetNode<Button>("HBoxContainer/StartButton").Pressed += () => OnStartButtonPressed(GameGlobals.GameMode.Normal);

		GetNode<Button>("HBoxContainer/ContinueButton").Pressed += () => OnStartButtonPressed(GameGlobals.GameMode.Continue);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnStartButtonPressed(GameGlobals.GameMode selectedMode)
	{
		var globals = GetNode<GameGlobals>("/root/GameGlobals");
		globals.CurrentMode = selectedMode;

		Error result = GetTree().ChangeSceneToFile(TDScenePath);

		if (result != Error.Ok)
		{
			GD.PrintErr("Failed to load scene: " + TDScenePath);
		}
	}
}
