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
		parentSpawner.IncreaseSpawnerMoneyBuff(_resource._moneyBuff);
		if (_resource._locations.Count > 0)
		{
			parentSpawner.UpdateSpawnerArea(_resource._locations);
		}
	}
}
