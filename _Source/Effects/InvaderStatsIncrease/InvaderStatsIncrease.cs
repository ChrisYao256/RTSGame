using Godot;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.Reflection;

public partial class InvaderStatsIncrease : Effect
{
	private InvaderStatsIncreaseResource _resource;

	public InvaderStatsIncrease(InvaderStatsIncreaseResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
	}

	//public void UpdateResource(SpawnerUnitStatsIncreaseResource newResource)
	//{
	//	_parentUnit._effects.Remove(_resource);

	//	if (!GodotObject.IsInstanceValid(this))
	//	{
	//		return;
	//	}
	//	if (!GodotObject.IsInstanceValid(_parentUnit))
	//	{
	//		QueueFree();
	//		return;
	//	}
	//	Spawner parentSpawner = (Spawner)_parentUnit;
	//	parentSpawner.RemoveSpawnerEnemies(_resource._units);
	//	parentSpawner.IncreaseSpawnerHpBuff(-_resource._hpBuff);
	//	parentSpawner.IncreaseSpawnerSpeedBuff(-_resource._speedBuff);
	//	parentSpawner.IncreaseSpawnerArmorBuff(-_resource._armorBuff);
	//	parentSpawner.IncreaseSpawnerMoneyBuff(-_resource._moneyBuff);
	//	parentSpawner._data._startingEffects = [];
	//	if (_resource._locations.Count > 0)
	//	{
	//		parentSpawner.ResetSpawnerArea();
	//	}

	//	_resource = newResource;
	//	_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);

	//	parentSpawner.AddSpawnerEnemies(_resource._units);
	//	parentSpawner.IncreaseSpawnerHpBuff(_resource._hpBuff);
	//	parentSpawner.IncreaseSpawnerSpeedBuff(_resource._speedBuff);
	//	parentSpawner.IncreaseSpawnerArmorBuff(_resource._armorBuff);
	//	parentSpawner.IncreaseSpawnerMoneyBuff(_resource._moneyBuff);
	//	if (_resource._locations.Count > 0)
	//	{
	//		parentSpawner.UpdateSpawnerArea(_resource._locations);
	//	}
	//	parentSpawner.AddStartingEffect(_resource._startingEffects);
	//	_parentUnit._effects.Add(_resource);
	//}

	protected override void OnCreation()
	{
		if (_parentUnit is not InvaderUnit)
		{
			throw new Exception("Parent is not an invader!");
		}
		InvaderUnit invader = (InvaderUnit)_parentUnit;
		invader.IncreaseHpMaxModifier(_resource._hpBuff);
		invader.IncreaseSpeedModifier(_resource._speedBuff);
		invader.IncreaseArmorModifier(_resource._armorBuff);
		invader.IncreaseMoneyModifier(_resource._moneyBuff);
		foreach (EffectResource effect in _resource._startingEffects)
		{
			invader.AddEffect(effect);
		}
		QueueFree();
	}

	//public override void RemoveEffectNode()
	//{
	//	if (!GodotObject.IsInstanceValid(this))
	//	{
	//		return;
	//	}
	//	if (!GodotObject.IsInstanceValid(_parentUnit))
	//	{
	//		QueueFree();
	//		return;
	//	}
	//	Spawner parentSpawner = (Spawner)_parentUnit;
	//	parentSpawner.RemoveSpawnerEnemies(_resource._units);
	//	parentSpawner.IncreaseSpawnerHpBuff(-_resource._hpBuff);
	//	parentSpawner.IncreaseSpawnerSpeedBuff(-_resource._speedBuff);
	//	parentSpawner.IncreaseSpawnerMoneyBuff(-_resource._moneyBuff);
	//	if (_resource._locations.Count > 0)
	//	{
	//		parentSpawner.ResetSpawnerArea();
	//	}
	//	_parentUnit._effects.Remove(_resource);
	//	QueueFree();
	//}
}
