using RTSGame.Units;
using System;
using System.Collections.Generic;
using Godot;
using RTSGame.Source;

public partial class NearbySpawnerStatsIncrease : Effect
{
	private NearbySpawnerStatsIncreaseResource _resource;
	private List<Spawner> _affectedSpawners = [];

	public NearbySpawnerStatsIncrease(NearbySpawnerStatsIncreaseResource resource) : base(resource)
	{
		_resource = resource;
		
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.PlacedTower, Callable.From<TowerUnit>(OnPlacedTower));
		OnCreation();
	}

	protected override void OnCreation()
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I location = parentTower._gridLocation;
		foreach (Vector2I relativePos in _resource._area)
		{
			Vector2I position = relativePos + location;
			if (!parentTower._grid.IsCellVacant(position) && relativePos != new Vector2I(0,0))
			{
				TowerUnit tower = parentTower._grid.GetTowerOnCell(position);
				if (tower is Spawner spawner)
				{
					for (int i = 0; i < spawner._spawnerData._units.Count; i++)
					{
						spawner.AddSpawnerUnitStatsIncrease(i, (InvaderStatsIncreaseResource)_resource._buffResource.DuplicateDeep());
					}
					_affectedSpawners.Add(spawner);
				}
			}
		}
	}

	protected override void OnPlacedTower(TowerUnit tower)
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I delta = tower._gridLocation - parentTower._gridLocation;
		if (_resource._area.Contains(delta) && delta != new Vector2I(0,0))
		{
			if (tower is Spawner spawner)
			{
				for (int i = 0; i < spawner._spawnerData._units.Count; i++)
				{
					spawner.AddSpawnerUnitStatsIncrease(i, (InvaderStatsIncreaseResource)_resource._buffResource.DuplicateDeep());
				}
				if (!_affectedSpawners.Contains(spawner))
				{
					_affectedSpawners.Add(spawner);
				}
			}
		}
	}

	public void AddNewBuffResource(InvaderStatsIncreaseResource resource)
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I location = parentTower._gridLocation;
		foreach (Vector2I relativePos in _resource._area)
		{
			Vector2I position = relativePos + location;
			if (!parentTower._grid.IsCellVacant(position) && relativePos != new Vector2I(0, 0))
			{
				TowerUnit tower = parentTower._grid.GetTowerOnCell(position);
				if (tower is Spawner spawner)
				{
					for (int i = 0; i < spawner._spawnerData._units.Count; i++)
					{
						spawner.AddSpawnerUnitStatsIncrease(i, (InvaderStatsIncreaseResource)resource.DuplicateDeep());
					}
					if (!_affectedSpawners.Contains(spawner))
					{
						_affectedSpawners.Add(spawner);
					}
				}
			}
		}
	}

	public override void RemoveEffectNode()
	{
		if (!GodotObject.IsInstanceValid(_parentUnit))
		{
			QueueFree();
			return;
		}
		foreach (Spawner spawner in _affectedSpawners)
		{
			for (int i = 0; i < spawner._spawnerData._units.Count; i++)
			{
				spawner.RemoveSpawnerUnitStatsIncrease(i, (InvaderStatsIncreaseResource)_resource._buffResource.DuplicateDeep());
			}
		}
		QueueFree();
	}
}
