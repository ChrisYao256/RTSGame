using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class MultiHitRampUpDamageResource : EffectResource
{
	[Export]
	public float _increaseAmount;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		MultiHitRampUpDamageResource typedOldResource = (MultiHitRampUpDamageResource)oldResource;

		typedOldResource._increaseAmount += _increaseAmount;
		typedOldResource.SetDescription();
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = $"Increases damage by {_increaseAmount * 100:F0}% when attacks hit the same enemy.";
	}

	public override void SetUpgradeDescription()
	{
		_effectDescription = $"Increases an extra +{_increaseAmount * 100:F2}% damage each time.";
	}

	public override Effect CreateNode()
	{
		return new MultiHitRampUpDamage(this);
	}
}
