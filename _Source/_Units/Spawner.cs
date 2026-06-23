using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using static Godot.OpenXRCompositionLayer;

namespace RTSGame.Units;

public partial class Spawner : TowerUnit
{
	[Export]
  public SpawnerDataResource _data;

	[Export]
	private double _spawnInterval = 1f;

	[Export]
	private bool _isEnemy = true;

	private Area2D _spawnArea;

	public override void _Ready()
	{
		_data = (SpawnerDataResource)_data.Duplicate(true);
		base._Ready();
		_tdManager = GetTree().CurrentScene.GetNode<TDManager>("TdManager");
		_spawnArea = GetNode<Area2D>("AttackArea");

		if (_data._units.Count == 1)
		{
			Unit unit = UnitManager.GetUnit(_data._units[0]._unitName, true);
			Texture2D icon = unit.GetIconTexture();
			Sprite2D sprite = new();
			sprite.Texture = icon;
			Utils.ScaleVisualToRadius(sprite, unit._radius);
			AddChild(sprite);
			unit.QueueFree();
		}
		else
		{
			for (int i =0; i < _data._units.Count; i++)
			{
				InvaderStatsIncreaseResource unitResource = _data._units[i];
				Unit unit = UnitManager.GetUnit(unitResource._unitName, true);
				Texture2D icon = unit.GetIconTexture();
				Sprite2D sprite = new();
				sprite.Texture = icon;
				float x = (float)Enumerable.Range(0, _data._units.Count).Select(i => (i - (_data._units.Count - 1) / 2.0) * unit._radius).ToArray()[i];
				sprite.Position = new Vector2(x, 0);
				Utils.ScaleVisualToRadius(sprite, unit._radius);
				AddChild(sprite);
				unit.QueueFree();
			}
		}

		if (_description == null || _description == "")
		{
			_description = "Enemies enter the battlefield through this portal each turn. Kill them to earn resources. ";
		}
	}

	public async void SpawnWave()
	{
		foreach (InvaderStatsIncreaseResource unitResource in _data._units)
		{
			string unit = unitResource._unitName;
			List<Vector2I> validSpawnLocations = [];
			Array<Vector2I> nearbyLocations = [new(1, 0), new(0, 1), new(0, -1), new(-1, 0)];
			foreach (Vector2I relativeMapPos in nearbyLocations)
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
			if (_isEnemy)
			{
				InvaderUnit invader = _tdManager.SpawnEnemyFromTower(unit, _gridLocation);
				invader.AddEffect(unitResource);
				await ToSignal(GetTree().CreateTimer(_spawnInterval), SceneTreeTimer.SignalName.Timeout);
			}
			else
			{
				Unit ally = _tdManager.SpawnAllyFromTower(unit, GlobalPosition);
			}

		}
	}

	public void OnNoValidSpawn(string unit)
	{
		_tdManager._towerManager.TransformTower(_gridLocation, "DepoweredSpawner", false);
		//_tdManager.AddEnemyToQueue(unit);
	}

	public void SetSpawnerEnemies(Array<InvaderStatsIncreaseResource> enemies)
	{
		_data._units = enemies;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void AddSpawnerEnemies(Array<InvaderStatsIncreaseResource> enemies)
	{
		SetSpawnerEnemies(_data._units + enemies);
	}

	public void RemoveSpawnerEnemies(Array<InvaderStatsIncreaseResource> enemies)
	{
		foreach (var resource in enemies)
		{
			_data._units.Remove(resource);
		}
		SetSpawnerEnemies(_data._units);
	}

	public void AddSpawnerUnitStatsIncrease(int index, InvaderStatsIncreaseResource resource)
	{
		resource.MergeWithOld(_data._units[index], []);
	}

	public void RemoveSpawnerUnitStatsIncrease(int index, InvaderStatsIncreaseResource resource)
	{
		resource.RemoveFromOld(_data._units[index]);
	}

	//public void SetSpawnerHpBuff(int hpBuff)
	//{
	//	_data._hpBuff = hpBuff;
	//	EmitSignal(SignalName.UpdateStatsInfo);
	//}

	//public void IncreaseSpawnerHpBuff(int change)
	//{
	//	SetSpawnerHpBuff(_data._hpBuff + change);
	//}

	//public void SetSpawnerSpeedBuff(float speedBuff)
	//{
	//	_data._speedBuff = speedBuff;
	//	EmitSignal(SignalName.UpdateStatsInfo);
	//}

	//public void IncreaseSpawnerSpeedBuff(float change)
	//{
	//	SetSpawnerSpeedBuff(_data._speedBuff + change);
	//}

	//public void SetSpawnerArmorBuff(int armorBuff)
	//{
	//	_data._armorBuff = armorBuff;
	//	EmitSignal(SignalName.UpdateStatsInfo);
	//}

	//public void IncreaseSpawnerArmorBuff(int change)
	//{
	//	SetSpawnerArmorBuff(_data._armorBuff + change);
	//}

	//public void AddStartingEffect(Array<EffectResource> effects)
	//{
	//	foreach (EffectResource effect in effects)
	//	{
	//		_data._startingEffects.Add(effect);
	//	}
	//	EmitSignal(SignalName.UpdateInfo);
	//}

	//public void SetSpawnerMoneyBuff(Vector4I change)
	//{
	//	_data._moneyBuff = change;
	//	EmitSignal(SignalName.UpdateStatsInfo);
	//}

	//public void IncreaseSpawnerMoneyBuff(Vector4I change)
	//{
	//	SetSpawnerMoneyBuff(_data._moneyBuff + change);
	//}

	public override void OnNewWave()
	{
		SpawnWave();
	}

	//public void ShowSpawnRange()
	//{
	//	_grid.DrawVisualTiles(_data._locations.ToList(), _gridLocation);
	//}

	public void HideSpawnRange()
	{
		_grid.HideVisualTiles();
	}

	public override Vector4I GetIncome()
	{
		Vector4I income = base.GetIncome();

		foreach (InvaderStatsIncreaseResource unitResource in _data._units)
		{
			InvaderUnit invader = unitResource.GetInvader();
			income += invader.GetMoneyDropped();
			invader.QueueFree();
		}
		return income;
	}

	public string GetSpawns() // Assumes all units of the same name have the same money drop
	{
		Godot.Collections.Dictionary<string, int> nameCountDict = new Godot.Collections.Dictionary<string, int>();
		List<InvaderUnit> invaders = [];
		foreach (InvaderStatsIncreaseResource unit in _data._units)
		{
			if (!nameCountDict.Keys.Contains(unit._unitName))
			{
				nameCountDict.Add(unit._unitName, 1);
				invaders.Add(unit.GetInvader());
			}
			else
			{
				nameCountDict[unit._unitName] += 1;
			}
		}
		string spawnText = "";
		for (int i = 0; i < nameCountDict.Keys.Count - 1; i++)
		{
			string name = nameCountDict.Keys.ElementAt(i);
			Vector4I income = invaders[i].GetMoneyDropped();
			spawnText += nameCountDict[name] + " " + UnitManager.InternalNameToName(name) + "\n(drops " + Utils.MakeMoneyText(income) + "), ";
		}
		string lastName = nameCountDict.Keys.ElementAt(nameCountDict.Keys.Count - 1);
		Vector4I lastIncome = invaders[nameCountDict.Keys.Count - 1].GetMoneyDropped();
		spawnText += nameCountDict[lastName] + " " + UnitManager.InternalNameToName(lastName) + "\n(drops " + Utils.MakeMoneyText(lastIncome) + "), ";
		foreach (Node var in invaders)
		{
			var.QueueFree();
		}
		return spawnText;
	}

	public override Texture2D GetIconTexture()
	{
		if (_data._units.Count > 0)
		{
			Unit unit = UnitManager.GetUnit(_data._units[0]._unitName, false);
			Texture2D texture = unit.GetIconTexture();
			unit.QueueFree();
			return texture;
		}
		else
		{
			return _iconTexture;
		}
	}
}

