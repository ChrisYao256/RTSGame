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

	/// <summary>
	/// Merging combines _healAmount if both are _isShield or both are not _isShield.
	/// </summary>
	/// <param name="oldResource"></param>
	/// <param name="allMatchingResource"></param>
	/// <returns></returns>
	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		foreach (EffectResource resource in allMatchingResource)
		{
			NearbyHealingResource typedResource = (NearbyHealingResource)resource;
			if (_isShield != typedResource._isShield)
			{
				continue;
			}
			else
			{
				typedResource._healAmount += _healAmount;
				typedResource.SetDescription();
				return false;
			}
		}
		return true;
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
