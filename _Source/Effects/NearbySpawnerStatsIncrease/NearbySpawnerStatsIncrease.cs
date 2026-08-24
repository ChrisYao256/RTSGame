using RTSGame.Units;
using System;
using Godot;
using Godot.Collections;
using RTSGame.Source;

public partial class NearbySpawnerStatsIncrease : Effect
{
	private NearbySpawnerStatsIncreaseResource _resource;
	private Array<Spawner> _affectedSpawners = [];

	public NearbySpawnerStatsIncrease(NearbySpawnerStatsIncreaseResource resource) : base(resource)
	{
		_resource = resource;
		
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.PlacedTower, Callable.From<TowerUnit>(OnPlacedTower));
		unit.Connect(Unit.SignalName.MovedTower, Callable.From<TowerUnit>(OnMovedTower));
		unit.Connect(Unit.SignalName.Movement, Callable.From<Vector2I, Vector2I>(OnMovement));
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

	protected override void OnMovement(Vector2I origin, Vector2I target)
	{
		foreach (Spawner spawner in _affectedSpawners)
		{
			if (IsInstanceValid(spawner))
			{
				for (int i = 0; i < spawner._spawnerData._units.Count; i++)
				{
					spawner.RemoveSpawnerUnitStatsIncrease(i, (InvaderStatsIncreaseResource)_resource._buffResource.DuplicateDeep());
				}
			}

		}
		_affectedSpawners = [];
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
				if (!_affectedSpawners.Contains(spawner))
				{
					_affectedSpawners.Add(spawner);
					for (int i = 0; i < spawner._spawnerData._units.Count; i++)
					{
						spawner.AddSpawnerUnitStatsIncrease(i, (InvaderStatsIncreaseResource)_resource._buffResource.DuplicateDeep());
					}
				}
			}
		}
	}

	protected override void OnMovedTower(TowerUnit tower)
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I delta = tower._gridLocation - parentTower._gridLocation;
		if (_resource._area.Contains(delta) && delta != new Vector2I(0, 0))
		{
			if (tower is Spawner spawner)
			{
				if (!_affectedSpawners.Contains(spawner))
				{
					_affectedSpawners.Add(spawner);
					for (int i = 0; i < spawner._spawnerData._units.Count; i++)
					{
						spawner.AddSpawnerUnitStatsIncrease(i, (InvaderStatsIncreaseResource)_resource._buffResource.DuplicateDeep());
					}
				}
			}
		}
		else
		{
			if (tower is Spawner spawner)
			{
				if (_affectedSpawners.Contains(spawner))
				{
					_affectedSpawners.Remove(spawner);
					if (IsInstanceValid(spawner))
					{
						for (int i = 0; i < spawner._spawnerData._units.Count; i++)
						{
							spawner.RemoveSpawnerUnitStatsIncrease(i, (InvaderStatsIncreaseResource)_resource._buffResource.DuplicateDeep());
						}
					}
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
			if (IsInstanceValid(spawner))
			{
				for (int i = 0; i < spawner._spawnerData._units.Count; i++)
				{
					spawner.RemoveSpawnerUnitStatsIncrease(i, (InvaderStatsIncreaseResource)_resource._buffResource.DuplicateDeep());
				}
			}
		}
		QueueFree();
	}
}
