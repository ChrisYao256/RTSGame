using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;

namespace RTSGame.Units;

public partial class SpeedUp : Effect
{
	double _cooldown;

	int _triggerTimes;

	SpeedUpResource _resource;

	public SpeedUp(SpeedUpResource resource) : base(resource)
	{
		_resource = resource;
		_cooldown = _resource._interval;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
	}

	public override void _PhysicsProcess(double delta)
	{
		_cooldown -= delta;
		if (_cooldown <= 0 && _triggerTimes < _resource._maxTriggers)
		{
			_triggerTimes++;
			_cooldown = _resource._interval;
			_parentUnit.IncreaseSpeedModifier(_resource._increase);
		}
	}
}
