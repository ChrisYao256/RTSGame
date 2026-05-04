using System;
using Godot;
using RTSGame.Units;

public partial class BattleScene : Node2D
{
	[Export] public PackedScene UnitManager;
	public override void _Ready()
	{
		UnitManager manager = UnitManager.Instantiate<UnitManager>();
		AddChild(manager);
	}
}
