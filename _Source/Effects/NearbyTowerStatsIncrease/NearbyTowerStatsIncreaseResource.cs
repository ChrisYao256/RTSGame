using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class NearbyTowerStatsIncreaseResource : EffectResource
{
	[Export]
	public StatsIncreaseResource _buffResource;

	[Export]
	public Array<Vector2I> _area = new Array<Vector2I>();

	private NearbyTowerStatsIncrease _effect;

	public NearbyTowerStatsIncreaseResource()
	{
		_displayType = DisplayTypes.Large;
	}

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		NearbyTowerStatsIncreaseResource oldTypedResource = (NearbyTowerStatsIncreaseResource)oldResource;
		_buffResource.MergeWithOld(oldTypedResource._buffResource, []);
		if (oldTypedResource._effect != null)
		{
			oldTypedResource._effect.AddNewBuffResource(_buffResource);
		}
		oldTypedResource._effectDescription = "";
		oldTypedResource.SetDescription();
		return false;
	}

	public override void SetDescription()
	{
		_effectDescription = "Buffs nearby spawners with the following: \n";
		_buffResource.SetDescription();
		_effectDescription += _buffResource._effectDescription;
	}

	public override Effect CreateNode()
	{
		_effect = new NearbyTowerStatsIncrease(this);
		return _effect;
	}
}
