using Godot;
using RTSGame.Units;
using System;

public partial class DropMoreMoneyOnKill : Effect
{
	DropMoreMoneyOnKillResource _resource;

	int _hitCounter = 0;

	public DropMoreMoneyOnKill(DropMoreMoneyOnKillResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.KilledUnit, Callable.From<Unit>(OnKilledEnemy));
	}

	protected override void OnKilledEnemy(Unit enemy)
	{
		if (enemy is InvaderUnit invader)
		{
			invader.SetMoneyTempModifierDMMOK(_resource._percentIncrease);
		}
	}
}