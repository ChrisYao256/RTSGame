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

	[Export]
	public string OptionsScenePath = "res://_Content/_Scenes/OptionsScene.tscn";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (GetNode<SaveManager>("/root/SaveManager").HasSavedGame())
		{
			GetNode<Button>("HBoxContainer/ContinueButton").Visible = true;
		}
		else 
		{
			GetNode<Button>("HBoxContainer/ContinueButton").Visible = false;
		}

		GetNode<Button>("HBoxContainer/TutorialButton").Pressed += () => OnStartButtonPressed(GameGlobals.GameMode.Tutorial);

		// Bind the Debug button to pass GameMode.Debug
		GetNode<Button>("HBoxContainer/TestingButton").Pressed += () => OnStartButtonPressed(GameGlobals.GameMode.Debug);

		GetNode<Button>("HBoxContainer/StartButton").Pressed += () => OnStartButtonPressed(GameGlobals.GameMode.Normal);

		GetNode<Button>("HBoxContainer/RogueButton").Pressed += () => OnStartButtonPressed(GameGlobals.GameMode.Rogue);

		GetNode<Button>("HBoxContainer/ContinueButton").Pressed += () => OnStartButtonPressed(GameGlobals.GameMode.Continue);

		GetNode<Button>("DeleteSaveButton").Pressed += () =>
		{
			GetNode<SaveManager>("/root/SaveManager").DeleteSave();
			GetNode<Button>("HBoxContainer/ContinueButton").Visible = false;
		};

		GetNode<Button>("HBoxContainer/QuitButton").Pressed += () =>
		{
			GetTree().Quit();
		};
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

	public void OnOptionsButtonPressed()
	{
		Error result = GetTree().ChangeSceneToFile(OptionsScenePath);

		if (result != Error.Ok)
		{
			GD.PrintErr("Failed to load scene: " + OptionsScenePath);
		}
	}
}
