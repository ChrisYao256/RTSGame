using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace RTSGame.Units;

[GlobalClass]
public partial class NearbySpawnerStatsIncreaseResource : EffectResource
{
	[Export]
	public InvaderStatsIncreaseResource _buffResource;

	[Export]
	public Array<Vector2I> _area = new Array<Vector2I>();

	private NearbySpawnerStatsIncrease _effect;

	public NearbySpawnerStatsIncreaseResource()
	{
		_displayType = DisplayTypes.Large;
	}

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource) 
	{
		NearbySpawnerStatsIncreaseResource oldTypedResource = (NearbySpawnerStatsIncreaseResource)oldResource;
		_buffResource.MergeWithOld(oldTypedResource._buffResource, []);
		if (_effect != null)
		{
			oldTypedResource._effect.AddNewBuffResource(_buffResource);
		}
		oldTypedResource._effectDescription = "";
		oldTypedResource.SetDescription();
		return false;
	}

	public override void SetDescription()
	{
		if (_effectDescription != "")
		{
			return;
		}
		_effectDescription = "Enemies from nearby portals get \n";

		if (_buffResource._hpBuff != 0)
		{
			_effectDescription += _buffResource._hpBuff * 100 + "% increased enemy HP\n";
		}

		if (_buffResource._speedBuff != 0)
		{
			_effectDescription += _buffResource._speedBuff + " increased speed\n";
		}

		if (_buffResource._moneyBuff != new Vector4I(0,0,0,0))
		{
			_effectDescription += "+" + Utils.MakeMoneyText(_buffResource._moneyBuff) + "\n";
		}
		if (_buffResource._startingEffects.Count > 0)
		{
			foreach (EffectResource effect in _buffResource._startingEffects)
			{
				effect.SetDescription();
				_effectDescription += effect._effectDescription + "\n";
			}
		}
	}

	public override Effect CreateNode()
	{
		_effect = new NearbySpawnerStatsIncrease(this);
		return _effect;
	}
}
