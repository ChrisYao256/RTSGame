using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class SpawnUnitOnDeath : Effect
{
	SpawnUnitOnDeathResource _resource;

	public SpawnUnitOnDeath(SpawnUnitOnDeathResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.Died, Callable.From(OnUnitDied));
		OnCreation();
	}

	protected override void OnUnitDied()
	{
		InvaderUnit invader = (InvaderUnit)_parentUnit;
		Array<Vector2> spawnPositions = [
				new Vector2(_resource._jitter, _resource._jitter),
				new Vector2(_resource._jitter, -_resource._jitter),
				new Vector2(-_resource._jitter, _resource._jitter),
				new Vector2(-_resource._jitter, -_resource._jitter),
				];

		foreach (InvaderStatsIncreaseResource unit in _resource._spawnedUnits)
		{
			Random random = new Random();
			int index = random.Next(0, spawnPositions.Count - 1);
			Vector2 specificSpawnPos = spawnPositions[index];

			Callable.From(() => {
				// This whole block executes deferred
				InvaderUnit newEnemy = invader._tdManager.SpawnEnemyAtGlobalPosition(unit._unitName, invader.GlobalPosition + specificSpawnPos);

				if (_resource._spawnedUnits != null)
				{
					// Apply your buffs directly to the returned reference
					newEnemy.IncreaseHpMaxModifier(unit._hpBuff);
					newEnemy.IncreaseSpeedModifier(unit._speedBuff);
					newEnemy.IncreaseArmorModifier(unit._armorBuff);
					newEnemy.SetMoneyModifier(-newEnemy.GetSelfMoneyDropped()); // spawned units with this effect always drop no money
					foreach (EffectResource effect in unit._startingEffects)
					{
						newEnemy.AddEffect(effect);
					}
				}
			}).CallDeferred();
			spawnPositions.RemoveAt(index);
		}
		MakeFloatingAnimation();
	}
}
