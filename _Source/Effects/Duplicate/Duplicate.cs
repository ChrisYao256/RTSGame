using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class Duplicate : Effect
{
	DuplicateResource _resource;

	public Duplicate(DuplicateResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
	}

	protected override void OnCreation()
	{
		Timer timer = new Timer();
		timer.OneShot = true;
		timer.WaitTime = _resource._delay;
		timer.Timeout += () =>
		{
			InvaderUnit invader = (InvaderUnit)_parentUnit;
			Array<Vector2> spawnPositions = [
					new Vector2(_resource._jitter, _resource._jitter),
				new Vector2(_resource._jitter, -_resource._jitter),
				new Vector2(-_resource._jitter, _resource._jitter),
				new Vector2(-_resource._jitter, -_resource._jitter),
				];

			Random random = new Random();
			int index = random.Next(0, spawnPositions.Count - 1);
			Vector2 specificSpawnPos = spawnPositions[index];

			Callable.From(() =>
			{
				// This whole block executes deferred
				InvaderUnit newEnemy = invader._tdManager.SpawnEnemyAtGlobalPosition(_parentUnit._internalName, invader.GlobalPosition + specificSpawnPos);

				// Apply your buffs directly to the returned reference
				newEnemy.IncreaseHpMaxModifier(_parentUnit._data._maxHpIncrease);
				newEnemy.IncreaseSpeedModifier(_parentUnit._data._speedIncrease);
				newEnemy.SetMoneyModifier(-newEnemy.GetSelfMoneyDropped()); // spawned units with this effect always drop no money
			}).CallDeferred();
		};
		AddChild(timer);
		timer.Start();
	}
}
