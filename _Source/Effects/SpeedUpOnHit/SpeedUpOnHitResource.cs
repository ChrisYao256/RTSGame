using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class SpeedUpOnHitResource : EffectResource
{
	[Export]
	public float _speedIncreasePerHit;

	[Export]
	public float _speedIncreaseCap;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		SpeedUpOnHitResource typedOldResource = (SpeedUpOnHitResource)oldResource;

		typedOldResource._speedIncreasePerHit += _speedIncreasePerHit;
		typedOldResource._speedIncreaseCap += _speedIncreaseCap;
		typedOldResource.SetDescription();
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = "Speeds up by " + _speedIncreasePerHit + " when hit. Max:"+ _speedIncreaseCap;
	}

	public override Effect CreateNode()
	{
		return new SpeedUpOnHit(this);
	}
}
