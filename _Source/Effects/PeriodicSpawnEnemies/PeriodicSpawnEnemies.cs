using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace RTSGame.Units;

public partial class PeriodicSpawnEnemies : Effect
{
	PeriodicSpawnEnemiesResource _resource;

	Timer _timer;

	public PeriodicSpawnEnemies(PeriodicSpawnEnemiesResource resource) : base(resource)
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
		if (!IsInsideTree())
		{
			QueueFree();
			return;
		}
		_timer = new Timer();
		_timer.Name = "Timer";
		AddChild(_timer);
		_timer.WaitTime = _resource._spawnInterval;
		_timer.OneShot = false;
		_timer.Timeout += () =>
		{
			_parentUnit.PauseNavigation();
			GetTree().CreateTimer(0.5f).Timeout += () => _parentUnit.ResumeNavigation();
			SpawnEnemies();
		};
		_timer.Start();
	}

	private void SpawnEnemies()
	{
		InvaderUnit invader = (InvaderUnit)_parentUnit;
		Array<Vector2> spawnPositions = [
				new Vector2(_resource._jitter, _resource._jitter),
				new Vector2(_resource._jitter, -_resource._jitter),
				new Vector2(-_resource._jitter, _resource._jitter),
				new Vector2(-_resource._jitter, -_resource._jitter),
				];

		foreach (InvaderStatsIncreaseResource unit in _resource._buffResource)
		{
			Random random = new Random();
			int index = random.Next(0, spawnPositions.Count - 1);
			Vector2 specificSpawnPos = spawnPositions[index];

			Callable.From(() => {
				// This whole block executes deferred
				InvaderUnit newEnemy = invader._tdManager.SpawnEnemyAtGlobalPosition(unit._unitName, invader.GlobalPosition + specificSpawnPos);
				newEnemy.AddEffect(unit);
			}).CallDeferred();
			spawnPositions.RemoveAt(index);
		}
		MakeFloatingAnimation();
	}
}
