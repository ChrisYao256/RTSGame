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
		unit.Connect(Unit.SignalName.HitEnemy, Callable.From<Unit>(OnHitEnemy));
	}

	protected override void OnHitEnemy(Unit enemy)
	{
		foreach (EffectResource debuff in _resource._debuffs)
		{
			enemy.AddEffect(debuff);
		}
		
	}
}