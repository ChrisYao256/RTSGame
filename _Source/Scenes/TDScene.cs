using System;
using Godot;
using RTSGame.Source;
using RTSGame.Units;

public partial class TDScene : Node2D
{
	[Export] public PackedScene UnitManager;

	private TDManager _tdManager;

	public override void _Ready()
	{
		UnitManager unitManager = UnitManager.Instantiate<UnitManager>();
		AddChild(unitManager);

		var globals = GetNode<GameGlobals>("/root/GameGlobals");

		_tdManager = GetNode<TDManager>("TdManager");
		_tdManager.Initialize(globals.CurrentMode);
	}

	public void OnNextWavePressed()
	{
		if (_tdManager.CheckWaveFinished())
		{
			_tdManager.SpawnNextWave();
		}
	}
}
