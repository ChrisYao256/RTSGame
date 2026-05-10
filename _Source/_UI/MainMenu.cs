using Godot;
using RTSGame.Units;
using System;

public partial class MainMenu : Control
{
	[Export]
	public string BattleScenePath = "res://_Content/_Scenes/BattleScene.tscn";

	[Export]
	public string TDScenePath = "res://_Content/_Scenes/TDScene.tscn";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnStartButtonPressed()
	{
		Error result = GetTree().ChangeSceneToFile(TDScenePath);

		if (result != Error.Ok)
		{
			GD.PrintErr("Failed to load scene: " + TDScenePath);
		}
	}
}
