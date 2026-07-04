using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class BlinkResource : EffectResource
{
	[Export]
	public float _distance;
	[Export] public float _tracerWidth = 2f;
	[Export] public Color _tracerColor = ThemePalette.Blue;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		BlinkResource typedOldResource = (BlinkResource)oldResource;

		typedOldResource._distance += _distance;
		typedOldResource.SetDescription();

		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;

		_effectDescription = $"Blinks across {_distance} when hit for the first time. ";
	}

	public override void SetUpgradeDescription()
	{
		_effectDescription = $"Increases blink distance. ";
	}

	public override Effect CreateNode()
	{
		return new Blink(this);
	}
}
