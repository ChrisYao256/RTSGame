using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class SelfHealingResource : EffectResource
{
	[Export]
	public int _healAmount;

	[Export]
	public float _healPercent;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		SelfHealingResource typedOldResource = (SelfHealingResource)oldResource;

		typedOldResource._healAmount += _healAmount;
		typedOldResource._healPercent += _healPercent;
		typedOldResource.SetDescription();
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		if (_healAmount > 0)
		{
			_effectDescription = "Regenerates " + _healAmount + " HP every second.";
		}
		else
		{
			_effectDescription = $"Regenerates {_healPercent * 100:F0}% HP every second.";
		}
			
	}

	public override Effect CreateNode()
	{
		return new SelfHealing(this);
	}
}
