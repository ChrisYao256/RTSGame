using Godot;
using RTSGame.Source;
using RTSGame.Units;
using Godot.Collections;
using System;
using System.Net.Sockets;

public partial class LevelsScene : Node2D
{
	[Export]
	public Array<LevelResource> Levels;

	public string TDScenePath = "res://_Content/_Scenes/TDScene.tscn";

	public override void _Ready()
	{

	}

	public void EnterLevel(int n)
	{
		var globals = GetNode<GameGlobals>("/root/GameGlobals");
		globals.CurrentLevel = Levels[n];

		Error result = GetTree().ChangeSceneToFile(TDScenePath);

		if (result != Error.Ok)
		{
			GD.PrintErr("Failed to load scene: " + TDScenePath);
		}
	}
}
