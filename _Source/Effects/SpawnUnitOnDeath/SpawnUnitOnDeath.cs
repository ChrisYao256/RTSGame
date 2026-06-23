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
		unit.Connect(Unit.SignalName.Died, Callable.From<Unit>(OnUnitDied));
		OnCreation();
	}

	protected override void OnUnitDied(Unit unit)
	{
		if (unit is InvaderUnit invader)
		{
			Array < Vector2 > spawnPositions = [
				new Vector2(_resource._jitter, _resource._jitter),
				new Vector2(_resource._jitter, -_resource._jitter),
				new Vector2(-_resource._jitter, _resource._jitter),
				new Vector2(-_resource._jitter, -_resource._jitter),
				];

			foreach (string unitName in _resource._spawnedUnit)
			{
				Random random = new Random();
				int index = random.Next(0, spawnPositions.Count - 1);
				Vector2 specificSpawnPos = spawnPositions[index];

				Callable.From(() => {
					// This whole block executes deferred
					InvaderUnit newEnemy = invader._tdManager.SpawnEnemyAtGlobalPosition(unitName, invader.GlobalPosition + specificSpawnPos);

					if (_resource._buffResource != null)
					{
						// Apply your buffs directly to the returned reference
						newEnemy.IncreaseHpMaxModifier(_resource._buffResource._hpBuff);
						newEnemy.IncreaseSpeedModifier(_resource._buffResource._speedBuff);
						newEnemy.IncreaseArmorModifier(_resource._buffResource._armorBuff);
						newEnemy.SetMoneyModifier(-newEnemy.GetMoneyDropped()); // spawned units with this effect always drop no money
						foreach (EffectResource effect in _resource._buffResource._startingEffects)
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
}
