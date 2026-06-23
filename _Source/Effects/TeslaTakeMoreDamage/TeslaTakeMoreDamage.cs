using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class TeslaTakeMoreDamage : Effect
{
	TeslaTakeMoreDamageResource _resource;	

	public TeslaTakeMoreDamage(TeslaTakeMoreDamageResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.BeforeIsHit, Callable.From<Unit>(OnBeforeIsHit));
		unit.Connect(Unit.SignalName.IsHit, Callable.From<Unit>(OnIsHit));
		Timer timer = new Timer();
		AddChild(timer);
		timer.WaitTime = _resource._duration;
		timer.OneShot = true;
		timer.Start();
		timer.Timeout += (() => {
			RemoveEffectResource();
			RemoveEffectNode();
			_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		});
	}

	protected override void OnBeforeIsHit(Unit unit)
	{
		if (unit._weapon is TeslaWeapon)
		{
			_parentUnit.IncreaseDamageTakenDebuff(_resource._damageIncrease);
		}
	}

	protected override void OnIsHit(Unit unit)
	{
		if (unit._weapon is TeslaWeapon)
		{
			_parentUnit.IncreaseDamageTakenDebuff(-_resource._damageIncrease);
		}
	}
}
