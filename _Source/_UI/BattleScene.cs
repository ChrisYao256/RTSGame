using System;
using Godot;
using RTSGame.Units;

public partial class BattleScene : Node2D
{
	[Export] public PackedScene UnitManager;
	[Export] public PackedScene UnitInfoPanel;
	public override void _Ready()
	{
		UnitInfoPanel infoPanel = UnitInfoPanel.Instantiate<UnitInfoPanel>();
		AddChild(infoPanel);
		UnitManager manager = UnitManager.Instantiate<UnitManager>();
		AddChild(manager);
	}
}
