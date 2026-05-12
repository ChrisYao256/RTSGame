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

	public SelfHealing(SelfHealingResource resource) : base(resource)
	{
		_resource = resource;
	}

	protected override void OnCreation()
	{
		_timer = new Timer();
		AddChild(_timer);
		_timer.WaitTime = _resource._healInterval;
		_timer.OneShot = false;
		_timer.Timeout += () => _parentUnit.IncreaseHp(_resource._healAmount);
		_timer.Start();
	}
}
