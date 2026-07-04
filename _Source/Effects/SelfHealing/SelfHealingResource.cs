using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class SelfHealingResource : EffectResource
{
	[Export]
	public int _healAmount;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		SelfHealingResource typedOldResource = (SelfHealingResource)oldResource;

		typedOldResource._healAmount += _healAmount;
		typedOldResource.SetDescription();
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = "Regenerates " + _healAmount + " Hp every second.";
	}

	public override Effect CreateNode()
	{
		return new SelfHealing(this);
	}
}
