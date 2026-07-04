using RTSGame.Units;
using System;
using System.Collections.Generic;
using Godot;
using RTSGame.Source;

public partial class NearbyTowerStatsIncrease : Effect
{
	private NearbyTowerStatsIncreaseResource _resource;
	private List<TowerUnit> _affectedTowers = [];

	public NearbyTowerStatsIncrease(NearbyTowerStatsIncreaseResource resource) : base(resource)
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
			if (!parentTower._grid.IsCellVacant(position) && relativePos != new Vector2I(0, 0))
			{
				TowerUnit tower = parentTower._grid.GetTowerOnCell(position);
				tower.AddEffect(_resource._buffResource);
				if (!_affectedTowers.Contains(tower))
				{
					_affectedTowers.Add(tower);
				}
			}
		}
	}

	protected override void OnPlacedTower(TowerUnit tower)
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I delta = tower._gridLocation - parentTower._gridLocation;
		if (_resource._area.Contains(delta) && delta != new Vector2I(0, 0))
		{
			tower.AddEffect(_resource._buffResource);
			if (!_affectedTowers.Contains(tower))
			{
				_affectedTowers.Add(tower);
			}
		}
	}

	public void AddNewBuffResource(StatsIncreaseResource resource)
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I location = parentTower._gridLocation;
		foreach (Vector2I relativePos in _resource._area)
		{
			Vector2I position = relativePos + location;
			if (!parentTower._grid.IsCellVacant(position) && relativePos != new Vector2I(0, 0))
			{
				TowerUnit tower = parentTower._grid.GetTowerOnCell(position);
				tower.AddEffect(resource);
				if (!_affectedTowers.Contains(tower))
				{
					_affectedTowers.Add(tower);
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
		foreach (TowerUnit tower in _affectedTowers)
		{
			tower.RemoveTowerStatsIncrease((StatsIncreaseResource)_resource._buffResource.DuplicateDeep());
		}
		QueueFree();
	}
}
