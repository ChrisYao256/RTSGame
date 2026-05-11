using RTSGame.Units;
using System;
using System.Collections.Generic;
using Godot;

public partial class SelfHealing : Effect
{
	private Timer _timer;

	private int _healAmount;

	private double _healInterval;

	//public SelfHealing(int healAmount, double healInterval)
	//{
	//	_healAmount = healAmount;
	//	_healInterval = healInterval;
	//}

	public SelfHealing(SelfHealingResource resource) : base(resource)
	{
		_healAmount = resource._healAmount;
		_healInterval = resource._healInterval;
	}

	protected override void OnCreation()
	{
		_timer = new Timer();
		AddChild(_timer);
		_timer.WaitTime = _healInterval;
		_timer.OneShot = false;
		_timer.Timeout += () => _parentUnit.IncreaseHp(_healAmount);
		_timer.Start();
	}
}
