using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class RampUpDamageResource : EffectResource
{
	[Export]
	public int _increaseAmount;

	[Export]
	public double _increaseInterval;

	[Export]
	public bool _oneTime = false;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		RampUpDamageResource typedOldResource = (RampUpDamageResource)oldResource;

		typedOldResource._increaseAmount += _increaseAmount;
		typedOldResource.SetDescription();
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = "Increases tower damage by " + _increaseAmount + " for every " + _increaseInterval + " seconds the tower has spent attacking the same enemy.";
	}

	public override void SetUpgradeDescription()
	{
		_effectDescription = $"Increases an extra +{_increaseAmount} damage each time.";
	}

	public override Effect CreateNode()
	{
		return new RampUpDamage(this);
	}
}
