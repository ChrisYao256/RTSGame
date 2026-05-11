using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RTSGame.Units;

public partial class Spawner : TowerUnit
{
	[Export]
  private SpawnerDataResource _data;

	[Export]
	private double _spawnJitterRadius = 15;

	private TDManager _tdManager;
	private Area2D _spawnArea;

	public override void _Ready()
	{
		base._Ready();
		_tdManager = GetTree().CurrentScene.GetNode<TDManager>("TdManager");
		_spawnArea = GetNode<Area2D>("AttackArea");
		_data = (SpawnerDataResource)_data.Duplicate();
	}

	public override void DisplayAttackRange()
	{
		ShowSpawnRange();
	}

	public override void HideAttackRange()
	{
		HideSpawnRange();
	}

	public void SpawnWave()
	{
		foreach (string unit in _data._units)
		{
			List<Vector2I> validSpawnLocations = [];
			foreach (Vector2I relativeMapPos in _data._locations)
			{
				TileData data = _grid.GetCellTileData(_gridLocation + relativeMapPos);
				if (data != null && (bool)data.GetCustomData("Path"))
				{
					validSpawnLocations.Add(_gridLocation + relativeMapPos);
				}
			}
			if (validSpawnLocations.Count == 0)
			{
				return;
			}
			int random = GD.RandRange(0, validSpawnLocations.Count - 1);
			Vector2I gridLocation = validSpawnLocations[random];
			Vector2 position = _grid.MapToGlobal(gridLocation);
			float dx = (float)GD.RandRange(-_spawnJitterRadius, _spawnJitterRadius);
			float dy = (float)GD.RandRange(-_spawnJitterRadius, _spawnJitterRadius);
			Vector2 jitter = new(dx, dy);
			InvaderUnit invader = _tdManager.SpawnEnemyFromTower(unit, position + jitter);
			invader.IncreaseHpMaxModifier(_data._hpBuff);
			invader.IncreaseSpeedModifier(_data._speedBuff);
			invader.IncreaseMoneyModifier(_data._moneyBuff);
			foreach (EffectResource effect in _data._startingEffects)
			{
				invader.AddEffect(effect);
			}
		}
	}

	public void SetSpawnerEnemies(Array<string> enemies)
	{
		_data._units = enemies;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void AddSpawnerEnemies(Array<string> enemies)
	{
		SetSpawnerEnemies(_data._units + enemies);
	}

	public void SetSpawnerHpBuff(float hpBuff)
	{
		_data._hpBuff = hpBuff;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void IncreaseSpawnerHpBuff(float change)
	{
		SetSpawnerHpBuff(_data._hpBuff + change);
	}

	public void SetSpawnerSpeedBuff(float speedBuff)
	{
		_data._speedBuff = speedBuff;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void IncreaseSpawnerSpeedBuff(float change)
	{
		SetSpawnerSpeedBuff(_data._speedBuff + change);
	}

	public void UpdateSpawnerArea(Array<Vector2I> newArea)
	{
		_data._locations = newArea;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void SetSpawnerMoneyBuff(int change)
	{
		_data._moneyBuff = change;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void IncreaseSpawnerMoneyBuff(int change)
	{
		SetSpawnerMoneyBuff(_data._moneyBuff + change);
	}

	public void OnNewWave()
	{
		SpawnWave();
	}

	public void ShowSpawnRange()
	{
		_grid.DrawVisualTiles(_data._locations.ToList(), _gridLocation);
	}

	public void HideSpawnRange()
	{
		_grid.HideVisualTiles();
	}
}

