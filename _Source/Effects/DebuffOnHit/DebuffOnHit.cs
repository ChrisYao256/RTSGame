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
		unit.Connect(Unit.SignalName.BeforeHitEnemy, Callable.From<Unit>(OnBeforeHitEnemy));
	}

	protected override void OnBeforeHitEnemy(Unit enemy)
	{
		foreach (EffectResource debuff in _resource._debuffs)
		{
			debuff._source = _parentUnit;
			enemy.AddEffect(debuff);
		}
	}
}