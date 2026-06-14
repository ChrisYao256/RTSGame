using Godot;
using Godot.Collections;
using System.Linq;
using System.Collections.Generic;

namespace RTSGame.Units;

[GlobalClass]
public partial class SpawnerStatsIncreaseResource : EffectResource
{
	[Export]
	public Array<string> _units = new Array<string>();

	[Export]
	public Array<Vector2I> _locations = new Array<Vector2I>();

	[Export]
	public int _hpBuff;

	[Export]
	public float _speedBuff;

	[Export]
	public int _armorBuff;

	[Export]
	public Vector4I _moneyBuff;

	[Export]
	public int _hpLossBuff;

	[Export]
	public Vector4I _moneyLossBuff;

	[Export]
	public Array<EffectResource> _startingEffects;

	public SpawnerStatsIncrease _effect;

	public SpawnerStatsIncreaseResource MultiplyEffect(int n)
	{
		SpawnerStatsIncreaseResource newResource = (SpawnerStatsIncreaseResource)Duplicate();
		if (_units.Count > 0)
		{
			Array<string> newUnits = [];
			for (int i = 0; i < n; i++)
			{
				newUnits.AddRange(_units);
			}
			newResource._units = newUnits;
		}
		newResource._hpBuff *= n;
		newResource._hpLossBuff *= n;
		newResource._armorBuff *= n;
		newResource._moneyBuff *= n;
		newResource._moneyLossBuff *= n;
		newResource._speedBuff *= n;
		newResource.SetDescription();
		return newResource;
	}

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		SpawnerStatsIncreaseResource typedOldResource = (SpawnerStatsIncreaseResource)oldResource;

		SpawnerStatsIncreaseResource typedOldResourceCopy = (SpawnerStatsIncreaseResource)typedOldResource.Duplicate();

		if (typedOldResource._effect is not null) // i.e., oldResource has been added to a unit
		{
			if (_locations.Count != 0)
			{
				typedOldResourceCopy._locations = _locations;
			}
			typedOldResourceCopy._hpBuff += _hpBuff;
			typedOldResourceCopy._speedBuff += _speedBuff;
			typedOldResourceCopy._moneyBuff += _moneyBuff;
			typedOldResourceCopy._moneyLossBuff += _moneyLossBuff;
			typedOldResourceCopy._hpLossBuff += _hpLossBuff;
			typedOldResourceCopy._startingEffects.AddRange(_startingEffects);

			typedOldResource._effect.UpdateResource(typedOldResourceCopy);
			typedOldResourceCopy._effect = typedOldResource._effect;
		}
		else //i.e., oldResource has not been added to a unit
		{
			if (_locations.Count != 0)
			{
				typedOldResource._locations = _locations;
			}
			typedOldResource._hpBuff += _hpBuff;
			typedOldResource._speedBuff += _speedBuff;
			typedOldResource._moneyBuff += _moneyBuff;
			typedOldResource._moneyLossBuff += _moneyLossBuff;
			typedOldResource._hpLossBuff += _hpLossBuff;
			typedOldResource._startingEffects.AddRange(_startingEffects);
		}

			return false;
	}

	public override void SetDescription()
	{
		_effectDescription = "";
		if (_units != null && _units.Count > 0)
		{
			
			_effectDescription += "Spawns " + _units.Count + " extra enemies.\n";
		}

		if (_hpBuff != 0)
		{
			
			_effectDescription += "Increase spawned enemy HP by " + _hpBuff + "\n";
		}

		if (_speedBuff != 0)
		{
			
			_effectDescription += "Increase spawned enemy speed by " + _speedBuff + "\n";
		}

		if (_armorBuff != 0)
		{

			_effectDescription += "Increase spawned enemy armor by " + _armorBuff + "\n";
		}

		if (_moneyBuff != new Vector4I(0,0,0,0))
		{
			
			_effectDescription += "Increase spawned enemy gold drop by " + Utils.MakeMoneyText(_moneyBuff) + "\n";
		}
		if (_locations.Count > 0)
		{
			
			_effectDescription += "Changes spawning area \n";
		}
		if (_startingEffects.Count > 0)
		{
			_effectDescription += "Gives spawned units the following effects: \n";
			foreach (EffectResource effect in _startingEffects)
			{
				effect.SetDescription();
				_effectDescription += effect._effectDescription + "\n";
			}
		}
	}

	public override Effect CreateNode()
	{
		_effect = new SpawnerStatsIncrease(this);
		return _effect;
	}
}
