using RTSGame.Units;
using System;
using Godot.Collections;
using Godot;
using RTSGame.Source;

public partial class NearbyTowerStatsIncrease : Effect
{
	private NearbyTowerStatsIncreaseResource _resource;
	private Array<TowerUnit> _affectedTowers = [];

	public NearbyTowerStatsIncrease(NearbyTowerStatsIncreaseResource resource) : base(resource)
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

	protected override void OnMovement(Vector2I origin, Vector2I target)
	{
		foreach (TowerUnit tower in _affectedTowers)
		{
			if (IsInstanceValid(tower))
			{
				tower.RemoveTowerStatsIncrease((StatsIncreaseResource)_resource._buffResource.DuplicateDeep());
			}
		}
		_affectedTowers = [];
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
			if (!_affectedTowers.Contains(tower))
			{
				tower.AddEffect(_resource._buffResource);
				_affectedTowers.Add(tower);
			}
		}
	}

	protected override void OnMovedTower(TowerUnit tower)
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I delta = tower._gridLocation - parentTower._gridLocation;
		if (_resource._area.Contains(delta) && delta != new Vector2I(0, 0))
		{
			if (!_affectedTowers.Contains(tower))
			{
				tower.AddEffect(_resource._buffResource);
				_affectedTowers.Add(tower);
			}
		}
		else
		{
			if (_affectedTowers.Contains(tower))
			{
				tower.RemoveTowerStatsIncrease((StatsIncreaseResource)_resource._buffResource.DuplicateDeep());
				_affectedTowers.Remove(tower);
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
			if (IsInstanceValid(tower))
			{
				tower.RemoveTowerStatsIncrease((StatsIncreaseResource)_resource._buffResource.DuplicateDeep());
			}
		}
		QueueFree();
	}
}
