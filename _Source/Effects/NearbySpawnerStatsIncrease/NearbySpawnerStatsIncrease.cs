using RTSGame.Units;
using System;
using System.Collections.Generic;
using Godot;
using RTSGame.Source;

public partial class NearbySpawnerStatsIncrease : Effect
{
	private NearbySpawnerStatsIncreaseResource _resource;
	private List<Effect> _createdEffects = [];

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
					Effect addedEffect = spawner.AddEffect(_resource._buffResource);
					_createdEffects.Add(addedEffect);
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
				Effect addedEffect = spawner.AddEffect(_resource._buffResource);
				_createdEffects.Add(addedEffect);
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
		foreach (Effect effect in _createdEffects)
		{
			effect.RemoveEffectNode();
		}
		_parentUnit._effects.Remove(_resource);
		QueueFree();
	}
}
