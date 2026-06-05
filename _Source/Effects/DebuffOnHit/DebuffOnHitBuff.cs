using Godot;
using RTSGame.Units;
using System;

public partial class DebuffOnHitBuff : Effect
{
	DebuffOnHitBuffResource _resource;

	public DebuffOnHitBuff(DebuffOnHitBuffResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
	}

	protected override void OnCreation()
	{
		DebuffOnHitResource resource = (DebuffOnHitResource)_parentUnit.GetEffect(typeof(DebuffOnHitResource));
		foreach (EffectResource debuff in _resource._debuffs)
		{
			if (debuff is EffectBuffResource buff)
			{
				buff.ApplyBuff(resource._debuffs);
			}
			else
			{
				resource._debuffs.Add(debuff);
			}
		}
		resource.SetDescription();
	}
}