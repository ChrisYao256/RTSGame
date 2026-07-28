using Godot;
using Godot.Collections;
using RTSGame.Source;
using System.Collections.Generic;
using System.Linq;

namespace RTSGame.Units;

[GlobalClass]
public partial class InvaderStatsIncreaseResource : EffectResource
{
	[Export]
	public string _unitName;

	[Export]
	public int _hpBuff;

	[Export]
	public float _percentHpBuff;

	[Export]
	public float _speedBuff;

	[Export]
	public float _percentSpeedBuff;

	[Export]
	public int _level;

	[Export]
	public Vector4I _moneyBuff;

	[Export]
	public float _percentMoneyBuff;

	[Export]
	public int _hpLossBuff;

	[Export]
	public Vector4I _moneyLossBuff;

	[Export]
	public Array<EffectResource> _startingEffects = [];

	public InvaderStatsIncrease _effect;

	public InvaderStatsIncreaseResource MultiplyEffect(int n)
	{
		InvaderStatsIncreaseResource newResource = (InvaderStatsIncreaseResource)Duplicate();
		newResource._level += _level;
		newResource._hpBuff *= n;
		newResource._percentHpBuff *= n;
		newResource._hpLossBuff *= n;
		newResource._moneyBuff *= n;
		newResource._percentMoneyBuff *= n;
		newResource._moneyLossBuff *= n;
		newResource._speedBuff *= n;
		newResource._percentSpeedBuff *= n;
		newResource.SetDescription();
		return newResource;
	}

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		InvaderStatsIncreaseResource typedOldResource = (InvaderStatsIncreaseResource)oldResource;

		typedOldResource._level += _level;
		typedOldResource._hpBuff += _hpBuff;
		typedOldResource._percentHpBuff += _percentHpBuff;
		typedOldResource._speedBuff += _speedBuff;
		typedOldResource._percentSpeedBuff += _percentSpeedBuff;
		typedOldResource._moneyBuff += _moneyBuff;
		typedOldResource._percentMoneyBuff += _percentMoneyBuff;
		typedOldResource._moneyLossBuff += _moneyLossBuff;
		typedOldResource._hpLossBuff += _hpLossBuff;

		foreach (EffectResource effect in _startingEffects)
		{
			Unit.AddEffectResourceToArray(typedOldResource._startingEffects, effect);
		}

		return false;
	}

	public void RemoveFromOld(EffectResource oldResource)
	{
		InvaderStatsIncreaseResource typedOldResource = (InvaderStatsIncreaseResource)oldResource;

		typedOldResource._level -= _level;
		typedOldResource._hpBuff -= _hpBuff;
		typedOldResource._percentHpBuff -= _percentHpBuff;
		typedOldResource._speedBuff -= _speedBuff;
		typedOldResource._percentSpeedBuff -= _percentSpeedBuff;
		typedOldResource._moneyBuff -= _moneyBuff;
		typedOldResource._percentMoneyBuff -= _percentMoneyBuff;
		typedOldResource._moneyLossBuff -= _moneyLossBuff;
		typedOldResource._hpLossBuff -= _hpLossBuff;
		foreach (EffectResource resource in _startingEffects)
		{
			typedOldResource._startingEffects.Remove(resource);
		}
	}

	public InvaderUnit GetInvader()
	{
		InvaderUnit invader = TDManager.GetEnemy(_unitName, true);
		invader.AddEffect(this);
		return invader;
	}

	public override void SetDescription()
	{
		_effectDescription = "";

		if (_hpBuff != 0)
		{
			_effectDescription += "Increase spawned enemy HP by " + _hpBuff + "\n";
		}

		if (_percentHpBuff != 0)
		{
			_effectDescription += $"Increase spawned enemy HP by {_percentHpBuff * 100:F0}%\n";
		}

		if (_speedBuff != 0)
		{
			_effectDescription += "Increase spawned enemy speed by " + _speedBuff + "\n";
		}

		if (_percentSpeedBuff != 0)
		{
			_effectDescription += $"Increase spawned enemy speed by {_percentSpeedBuff * 100:F0}%\n";
		}

		if (_moneyBuff != new Vector4I(0,0,0,0))
		{
			_effectDescription += "Increase spawned enemy resources drop by " + Utils.MakeMoneyText(_moneyBuff) + "\n";
		}

		if (_level != 0)
		{
			_effectDescription += "Increase spawned enemy level by " + _level + "\n";
		}

		if (_percentMoneyBuff != 0)
		{
			_effectDescription += $"Increase spawned enemy resources drop by {_percentMoneyBuff * 100:F0}%\n";
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
		_effect = new InvaderStatsIncrease(this);
		return _effect;
	}
}
