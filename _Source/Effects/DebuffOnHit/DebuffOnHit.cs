using Godot;
using RTSGame.Units;
using System;

public partial class DebuffOnHit : Effect
{
	DebuffOnHitResource _resource;

	public DebuffOnHit(DebuffOnHitResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.BeforeHitEnemy, Callable.From<DamageContext>(OnBeforeHitEnemy));
	}

	protected override void OnBeforeHitEnemy(DamageContext damageContext)
	{
		foreach (EffectResource debuff in _resource._debuffs)
		{
			debuff._source = _parentUnit;
			damageContext._target.AddEffect(debuff);
		}
	}
}