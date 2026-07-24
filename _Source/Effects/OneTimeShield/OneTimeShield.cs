using Godot;
using RTSGame.Units;
using System;

public partial class OneTimeShield : Effect
{
	OneTimeShieldResource _resource;

	int _hitCounter = 0;

	public OneTimeShield(OneTimeShieldResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.BeforeIsHit, Callable.From<Unit>(OnBeforeIsHit));
		unit.Connect(Unit.SignalName.IsHit, Callable.From<Unit>(OnIsHit));
	}

	protected override void OnBeforeIsHit(Unit enemy)
	{
		_hitCounter++;
		if (_hitCounter <= _resource._stack)
		{
			_parentUnit.IncreaseDamageReductionBuff(1.0f);
		}
	}

	protected override void OnIsHit(Unit enemy)
	{
		if (_hitCounter <= _resource._stack)
		{
			_parentUnit.IncreaseDamageReductionBuff(-1.0f);
		}
	}
}