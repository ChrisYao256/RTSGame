using Godot;
using Godot.NativeInterop;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class NearbyHealingResource : EffectResource
{
	[Export]
	public int _healAmount;

	[Export]
	public double _healInterval;

	[Export]
	public float _radius;

	[Export]
	public PackedScene _healingVisualScene;

	[Export]
	public bool _isShield;

	private NearbyHealing _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		NearbyHealingResource typedOldResource = (NearbyHealingResource)oldResource;
		if (_isShield != typedOldResource._isShield)
		{
			return true;
		}
		else
		{
			typedOldResource._healAmount += _healAmount;
			typedOldResource._effect.UpdateResource(typedOldResource);
			typedOldResource.SetDescription();
			return false;
		}
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = $"{(_isShield? "Shields" : "Heals")} all units in a " + _radius + " radius for " + _healAmount + " every " + _healInterval + " seconds";
	}

	public override Effect CreateNode()
	{
		_effect = new NearbyHealing(this);
		return _effect;
	}
}
