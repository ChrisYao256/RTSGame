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

	public InvaderStatsIncrease _effect;

	public InvaderStatsIncreaseResource MultiplyEffect(int n)
	{
		InvaderStatsIncreaseResource newResource = (InvaderStatsIncreaseResource)Duplicate();
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
		InvaderStatsIncreaseResource typedOldResource = (InvaderStatsIncreaseResource)oldResource;

		typedOldResource._hpBuff += _hpBuff;
		typedOldResource._speedBuff += _speedBuff;
		typedOldResource._moneyBuff += _moneyBuff;
		typedOldResource._moneyLossBuff += _moneyLossBuff;
		typedOldResource._hpLossBuff += _hpLossBuff;
		typedOldResource._startingEffects.AddRange(_startingEffects);

		return false;
	}

	public void RemoveFromOld(EffectResource oldResource)
	{
		InvaderStatsIncreaseResource typedOldResource = (InvaderStatsIncreaseResource)oldResource;

		typedOldResource._hpBuff -= _hpBuff;
		typedOldResource._speedBuff -= _speedBuff;
		typedOldResource._moneyBuff -= _moneyBuff;
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
		//invader.IncreaseSpeedModifier(_speedBuff);
		//invader.IncreaseArmorModifier(_armorBuff);
		//invader.IncreaseMoneyModifier(_moneyBuff);
		//foreach (EffectResource effect in _startingEffects)
		//{
		//	invader.AddEffect(effect);
		//}
		return invader;
	}

	public override void SetDescription()
	{
		_effectDescription = "";

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
