using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace RTSGame.Units;

[GlobalClass]
public partial class NearbySpawnerStatsIncreaseResource : EffectResource
{
	[Export]
	public SpawnerStatsIncreaseResource _buffResource;

	[Export]
	public Array<Vector2I> _area = new Array<Vector2I>();

	private NearbySpawnerStatsIncrease _effect;

	public NearbySpawnerStatsIncreaseResource()
	{
		_displayType = DisplayTypes.Large;
	}

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource) 
	{
		// this updates the description of the first old resource and then secretly adds the new resource.
		// So there will be multiple NearbyStatsIncrease Resource, but all their stats get summed when displayed instead of individually displaying.
		NearbySpawnerStatsIncreaseResource newResource = (NearbySpawnerStatsIncreaseResource)DuplicateDeep();
		foreach (NearbySpawnerStatsIncreaseResource resource in allMatchingResource)
		{
			resource._buffResource.MergeWithOld(newResource._buffResource, []);
		}
		newResource.SetDescription();
		oldResource._effectDescription = newResource._effectDescription;

		_displayType = DisplayTypes.Hidden;
		return true;
	}

	public override void SetDescription()
	{
		_effectDescription = "Buffs nearby spawners with the following: \n";
		if (_buffResource._units != null && _buffResource._units.Count > 0)
		{
			_effectDescription += "Spawns " + _buffResource._units.Count + " extra enemies.\n";
		}

		if (_buffResource._hpBuff != 0)
		{
			_effectDescription += "Increase spawned enemy HP by " + _buffResource._hpBuff * 100 + "%\n";
		}

		if (_buffResource._speedBuff != 0)
		{
			_effectDescription += "Increase spawned enemy speed by " + _buffResource._speedBuff + "\n";
		}

		if (_buffResource._moneyBuff != new Vector4I(0,0,0,0))
		{
			_effectDescription += "Increase spawned enemy gold drop by " + Utils.MakeMoneyText(_buffResource._moneyBuff) + "\n";
		}
		if (_buffResource._locations.Count > 0)
		{
			_effectDescription += "Changes spawning area \n";
		}
		if (_buffResource._startingEffects.Count > 0)
		{
			_effectDescription += "Gives spawned units the following effects: \n";
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
