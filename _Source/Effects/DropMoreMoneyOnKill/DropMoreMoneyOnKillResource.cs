using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class DropMoreMoneyOnKillResource : EffectResource
{
	[Export] public float _percentIncrease;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		DropMoreMoneyOnKillResource typedOldResource = (DropMoreMoneyOnKillResource)oldResource;

		typedOldResource._percentIncrease += _percentIncrease;
		typedOldResource.SetDescription();

		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = $"Enemies killed by this drops {_percentIncrease * 100:F0}% more resources.";
	}

	public override Effect CreateNode()
	{
		return new DropMoreMoneyOnKill(this);
	}
}
