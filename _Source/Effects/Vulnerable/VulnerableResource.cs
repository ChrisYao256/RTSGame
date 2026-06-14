using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class VulnerableResource : EffectResource
{
	[Export]
	public float _percentIncrease;

	[Export]
	public float _time;

	private Vulnerable _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		VulnerableResource typedOldResource = (VulnerableResource)oldResource;
		typedOldResource._effect.AddResource(this);
		return false;
	}

	public override void SetDescription()
	{
		_effectName = "Vulnerable";
		_effectDescription = "Increase all damage taken by " + (_percentIncrease * 100).ToString() + "%";
	}

	public override Effect CreateNode()
	{
		_effect = new Vulnerable(this);
		return _effect;
	}
}
