using Godot;
using RTSGame.Source;
using RTSGame.Units;
using System;
using System.Reflection.Emit;

public partial class TDScene : Node2D
{
	[Export] public PackedScene UnitManager;

	private TDManager _tdManager;

	public override void _Ready()
	{
		UnitManager unitManager = UnitManager.Instantiate<UnitManager>();
		AddChild(unitManager);

		_tdManager = GetNode<TDManager>("TdManager");
		Callable.From(() =>
		{
			_tdManager.Initialize(GameGlobals.Instance.CurrentMode, GameGlobals.Instance.CurrentLevel);
		}).CallDeferred();
	}

	public void OnNextWavePressed()
	{
		if (_tdManager.CheckWaveFinished())
		{
			_tdManager.SpawnNextWave();
		}
	}
}
