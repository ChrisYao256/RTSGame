using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class DamageOverTimeResource : EffectResource
{
	[Export]
	public int _damage;

	[Export]
	public float _time;

	[Export]
	public PackedScene _burnVisualScene;

	private DamageOverTime _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		DamageOverTimeResource typedOldResource = (DamageOverTimeResource)oldResource;

		typedOldResource._effect.AddResource(this);
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Small;
		_effectName = "Burn";
		_effectDescription = "Taking " + _damage.ToString() + " every second";
	}

	public override Effect CreateNode()
	{
		_effect = new DamageOverTime(this);
		return _effect;
	}
}
