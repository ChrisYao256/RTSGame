using RTSGame.Units;
using System;
using System.Collections.Generic;
using Godot;

public partial class SpawnerStatsIncrease : Effect
{
	private SpawnerStatsIncreaseResource _resource;

	public SpawnerStatsIncrease(SpawnerStatsIncreaseResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
	}

	public void UpdateResource(SpawnerStatsIncreaseResource newResource)
	{
		_parentUnit._effects.Remove(_resource);

		if (!GodotObject.IsInstanceValid(this))
		{
			return;
		}
		if (!GodotObject.IsInstanceValid(_parentUnit))
		{
			QueueFree();
			return;
		}
		Spawner parentSpawner = (Spawner)_parentUnit;
		parentSpawner.RemoveSpawnerEnemies(_resource._units);
		parentSpawner.IncreaseSpawnerHpBuff(-_resource._hpBuff);
		parentSpawner.IncreaseSpawnerSpeedBuff(-_resource._speedBuff);
		parentSpawner.IncreaseSpawnerArmorBuff(-_resource._armorBuff);
		parentSpawner.IncreaseSpawnerMoneyBuff(-_resource._moneyBuff);
		parentSpawner._data._startingEffects = [];
		if (_resource._locations.Count > 0)
		{
			parentSpawner.ResetSpawnerArea();
		}

		_resource = newResource;
		_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);

		parentSpawner.AddSpawnerEnemies(_resource._units);
		parentSpawner.IncreaseSpawnerHpBuff(_resource._hpBuff);
		parentSpawner.IncreaseSpawnerSpeedBuff(_resource._speedBuff);
		parentSpawner.IncreaseSpawnerArmorBuff(_resource._armorBuff);
		parentSpawner.IncreaseSpawnerMoneyBuff(_resource._moneyBuff);
		if (_resource._locations.Count > 0)
		{
			parentSpawner.UpdateSpawnerArea(_resource._locations);
		}
		parentSpawner.AddStartingEffect(_resource._startingEffects);
		_parentUnit._effects.Add(_resource);
	}

	protected override void OnCreation()
	{
		if (_parentUnit is not Spawner)
		{
			throw new Exception("Parent is not a spawner!");
		}
		Spawner parentSpawner = (Spawner)_parentUnit;
		parentSpawner.AddSpawnerEnemies(_resource._units);
		parentSpawner.IncreaseSpawnerHpBuff(_resource._hpBuff);
		parentSpawner.IncreaseSpawnerSpeedBuff(_resource._speedBuff);
		parentSpawner.IncreaseSpawnerArmorBuff(_resource._armorBuff);
		parentSpawner.IncreaseSpawnerMoneyBuff(_resource._moneyBuff);
		if (_resource._locations.Count > 0)
		{
			parentSpawner.UpdateSpawnerArea(_resource._locations);
		}
		parentSpawner.AddStartingEffect(_resource._startingEffects);
	}

	public override void RemoveEffectNode()
	{
		if (!GodotObject.IsInstanceValid(this))
		{
			return;
		}
		if (!GodotObject.IsInstanceValid(_parentUnit))
		{
			QueueFree();
			return;
		}
		Spawner parentSpawner = (Spawner)_parentUnit;
		parentSpawner.RemoveSpawnerEnemies(_resource._units);
		parentSpawner.IncreaseSpawnerHpBuff(-_resource._hpBuff);
		parentSpawner.IncreaseSpawnerSpeedBuff(-_resource._speedBuff);
		parentSpawner.IncreaseSpawnerMoneyBuff(-_resource._moneyBuff);
		if (_resource._locations.Count > 0)
		{
			parentSpawner.ResetSpawnerArea();
		}
		_parentUnit._effects.Remove(_resource);
		QueueFree();
	}
}
