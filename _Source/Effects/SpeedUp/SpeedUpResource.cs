using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class SpeedUpResource : EffectResource
{
	[Export]
	public float _increase;

	[Export]
	public float _interval;

	[Export]
	public int _maxTriggers;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		SpeedUpResource typedOldResource = (SpeedUpResource)oldResource;

		typedOldResource._increase += _increase;
		typedOldResource._maxTriggers += _maxTriggers;
		typedOldResource.SetDescription();
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = $"Increase speed by {_increase} every {_interval} ::duration::, up to {_maxTriggers} times";
	}

	public override Effect CreateNode()
	{
		return new SpeedUp(this);
	}
}
