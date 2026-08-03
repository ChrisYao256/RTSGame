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

		Vector2 position = invader.GlobalPosition;

		foreach (InvaderStatsIncreaseResource unit in _resource._spawnedUnits)
		{
			Random random = new Random();
			int index = random.Next(0, spawnPositions.Count - 1);
			Vector2 specificSpawnPos = spawnPositions[index];

			Callable.From(() => {
				// This whole block executes deferred
				InvaderUnit newEnemy = invader._tdManager.SpawnEnemyAtGlobalPosition(unit._unitName, position + specificSpawnPos);

				if (_resource._spawnedUnits != null)
				{
					newEnemy.AddEffect(unit);
				}
			}).CallDeferred();
			spawnPositions.RemoveAt(index);
		}
		MakeFloatingAnimation();
	}
}
