using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static Godot.OpenXRCompositionLayer;

namespace RTSGame.Units;

public partial class Spawner : TowerUnit
{
	[Export]
  public SpawnerDataResource _data;

	[Export]
	private double _spawnJitterRadius = 15;

	[Export]
	private bool _isEnemy = true;

	private Array<Vector2I> _defaultLocations;

	private Area2D _spawnArea;

	public override void _Ready()
	{
		_defaultLocations = _data._locations;
		_data = (SpawnerDataResource)_data.Duplicate(true);
		base._Ready();
		_tdManager = GetTree().CurrentScene.GetNode<TDManager>("TdManager");
		_spawnArea = GetNode<Area2D>("AttackArea");
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
				OnNoValidSpawn(unit);
				return;
			}
			int random = GD.RandRange(0, validSpawnLocations.Count - 1);
			Vector2I gridLocation = validSpawnLocations[random];
			Vector2 position = _grid.MapToGlobal(gridLocation);
			float dx = (float)GD.RandRange(-_spawnJitterRadius, _spawnJitterRadius);
			float dy = (float)GD.RandRange(-_spawnJitterRadius, _spawnJitterRadius);
			Vector2 jitter = new(dx, dy);
			if (_isEnemy)
			{
				InvaderUnit invader = _tdManager.SpawnEnemyFromTower(unit, position + jitter);
				invader.IncreaseHpMaxModifier(_data._hpBuff);
				invader.IncreaseSpeedModifier(_data._speedBuff);
				invader.IncreaseArmorModifier(_data._armorBuff);
				invader.IncreaseMoneyModifier(_data._moneyBuff);
				foreach (EffectResource effect in _data._startingEffects)
				{
					invader.AddEffect(effect);
				}
			}
			else
			{
				Unit ally = _tdManager.SpawnAllyFromTower(unit, position + jitter);
			}
		}
	}

	public void OnNoValidSpawn(string unit)
	{
		_tdManager._towerManager.TransformTower(_gridLocation, "DepoweredSpawner");
		//_tdManager.AddEnemyToQueue(unit);
	}

	public void SetSpawnerEnemies(Array<string> enemies)
	{
		_data._units = enemies;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void AddSpawnerEnemies(Array<string> enemies)
	{
		SetSpawnerEnemies(_data._units + enemies);
	}

	public void RemoveSpawnerEnemies(Array<string> enemies)
	{
		foreach (string name in enemies)
		{
			_data._units.Remove(name);
		}
		SetSpawnerEnemies(_data._units);
	}

	public void SetSpawnerHpBuff(float hpBuff)
	{
		_data._hpBuff = hpBuff;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseSpawnerHpBuff(float change)
	{
		SetSpawnerHpBuff(_data._hpBuff + change);
	}

	public void SetSpawnerSpeedBuff(float speedBuff)
	{
		_data._speedBuff = speedBuff;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseSpawnerSpeedBuff(float change)
	{
		SetSpawnerSpeedBuff(_data._speedBuff + change);
	}

	public void SetSpawnerArmorBuff(int armorBuff)
	{
		_data._armorBuff = armorBuff;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseSpawnerArmorBuff(int change)
	{
		SetSpawnerArmorBuff(_data._armorBuff + change);
	}

	public void UpdateSpawnerArea(Array<Vector2I> newArea)
	{
		_data._locations = newArea;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void ResetSpawnerArea()
	{
		_data._locations = _defaultLocations;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void AddStartingEffect(Array<EffectResource> effects)
	{
		foreach (EffectResource effect in effects)
		{
			_data._startingEffects.Add(effect);
		}
		EmitSignal(SignalName.UpdateInfo);
	}

	public void SetSpawnerMoneyBuff(int change)
	{
		_data._moneyBuff = change;
		EmitSignal(SignalName.UpdateStatsInfo);
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

	public override int GetIncome()
	{
		int income = base.GetIncome();

		foreach (string unit in _data._units)
		{
			InvaderUnit invader = _tdManager.GetEnemy(unit);
			invader.IncreaseMoneyModifier(_data._moneyBuff);
			income += invader.GetMoneyDropped();
		}
		return income;
	}

	public string GetSpawns()
	{
		Godot.Collections.Dictionary<string, int> spawns = new Godot.Collections.Dictionary<string, int>();
		foreach (string unit in _data._units)
		{
			if (!spawns.Keys.Contains(unit))
			{
				spawns.Add(unit, 1);
			}
			else
			{
				spawns[unit] += 1;
			}
		}
		string spawnText = "";
		for (int i = 0; i < spawns.Keys.Count - 1; i++)
		{
			string name = spawns.Keys.ElementAt(i);
			InvaderUnit invader = _tdManager.GetEnemy(name);
			invader.IncreaseMoneyModifier(_data._moneyBuff);
			int income = invader.GetMoneyDropped();
			spawnText += spawns[name] + " " + UnitManager.InternalNameToName(name) + " ($" + income + "), ";
		}
		string lastName = spawns.Keys.ElementAt(spawns.Keys.Count - 1);
		InvaderUnit lastInvader = _tdManager.GetEnemy(lastName);
		lastInvader.IncreaseMoneyModifier(_data._moneyBuff);
		int lastIncome = lastInvader.GetMoneyDropped();
		spawnText += spawns[lastName] + " " + UnitManager.InternalNameToName(lastName) + " ($" + lastIncome + ")";
		return spawnText;
	}
}

