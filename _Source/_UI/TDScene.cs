using System;
using Godot;
using RTSGame.Source;
using RTSGame.Units;

public partial class TDScene : Node2D
{
	[Export] public PackedScene UnitManager;
	[Export] public PackedScene UnitInfoPanel;

	private TDManager _tdManager;

	public override void _Ready()
	{
		UnitInfoPanel infoPanel = UnitInfoPanel.Instantiate<UnitInfoPanel>();
		AddChild(infoPanel);
		UnitManager unitManager = UnitManager.Instantiate<UnitManager>();
		AddChild(unitManager);
		_tdManager = GetNode<TDManager>("TdManager");
		_tdManager.Initialize();
	}

	public void OnNextWavePressed()
	{
		_tdManager.SpawnNextWave();
	}
}
