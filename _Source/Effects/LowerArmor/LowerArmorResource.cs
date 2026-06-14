using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class LowerArmorResource : EffectResource
{
	[Export]
	public int _change;

	[Export]
	public float _time;

	private LowerArmor _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		LowerArmorResource typedOldResource = (LowerArmorResource)oldResource;
		typedOldResource._effect.AddResource(this);
		return false;
	}

	public override void SetDescription()
	{
		_effectName = "Armor Lowered";
		_effectDescription = "Armor lowered by " + _change.ToString();
	}

	public override Effect CreateNode()
	{
		_effect = new LowerArmor(this);
		return _effect;
	}
}
