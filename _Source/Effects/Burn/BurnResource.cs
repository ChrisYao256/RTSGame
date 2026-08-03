using Godot;
using RTSGame._Source.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class BurnResource : EffectResource, IStatus
{
	[Export]
	public int _damage;

	[Export]
	public PackedScene _burnVisualScene;

	public float _burnIntervalMultiplier = 1f;

	public const float BaseBurnInterval = 1f;

	public float _decayRate = 0.03f;

	private Burn _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		BurnResource typedOldResource = (BurnResource)oldResource;

		if (typedOldResource._effect is not null)
		{
			typedOldResource._effect.AddResource(this);
		}
		else
		{
			typedOldResource._damage += _damage;
			typedOldResource.SetDescription();
		}
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Small;
		_effectName = "Burning " + (_damage).ToString("F0");
		_effectDescription = "Take " + _damage.ToString() + $" damage every second. This amount decreases by {_decayRate * 100:F0}% of target's max HP every second.";
	}

	public override Effect CreateNode()
	{
		_effect = new Burn(this);
		return _effect;
	}
}
