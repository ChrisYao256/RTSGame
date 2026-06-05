using RTSGame.Units;
using System;
using System.Collections.Generic;
using Godot;

public partial class SelfHealing : Effect
{
	private Timer _timer;

	//private int _healAmount;

	//private double _healInterval;

	SelfHealingResource _resource;

	float _queuedHealing;

	public SelfHealing(SelfHealingResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void _PhysicsProcess(double delta)
	{
		_queuedHealing += (float)delta * _resource._healAmount;
		if (_queuedHealing > 1)
		{
			_parentUnit.IncreaseHp((int)Mathf.FloorToInt(_queuedHealing), true);
			_queuedHealing -= Mathf.FloorToInt(_queuedHealing);
		}
	}
}
