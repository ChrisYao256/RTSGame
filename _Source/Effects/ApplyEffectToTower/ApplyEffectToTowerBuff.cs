using Godot;
using RTSGame.Units;
using System;

public partial class ApplyEffectToTowerBuff : Effect
{
	ApplyEffectToTowerBuffResource _resource;

	public ApplyEffectToTowerBuff(ApplyEffectToTowerBuffResource resource) : base(resource)
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
		ApplyEffectToTowerBuffResource resource = (ApplyEffectToTowerBuffResource)_parentUnit.GetEffect(typeof(ApplyEffectToTowerBuffResource));
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