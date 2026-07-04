using Godot;
using RTSGame._Source.Units;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class StatsIncreaseResource : EffectResource, IStackable
{
	[Export]
	public int _damageIncrease;

	[Export]
	public float _damagePercentIncrease;

	[Export]
	public double _attackSpeedIncrease;

	[Export]
	public int _maxHpIncrease;

	[Export]
	public float _rangeIncrease;

	[Export]
	public float _speedIncrease;

	[Export]
	public double _attackDelayModifierIncrease;

	[Export]
	public bool _disableDelay;

	[Export]
	public int _pierceCount;

	[Export]
	public ConvexPolygonShape2D _newZone;

	public EffectResource MultiplyEffect(int n)
	{
		StatsIncreaseResource newResource = (StatsIncreaseResource)Duplicate();
		newResource._damageIncrease *= n;
		newResource._damagePercentIncrease *= n;
		newResource._rangeIncrease *= n;
		newResource._attackSpeedIncrease *= n;
		newResource._speedIncrease *= n;
		newResource._attackDelayModifierIncrease *= n;
		newResource._pierceCount *= n;
		newResource.SetDescription();
		return newResource;
	}

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		StatsIncreaseResource typedOldResource = (StatsIncreaseResource)oldResource;

		typedOldResource._damageIncrease += _damageIncrease;
		typedOldResource._damagePercentIncrease += _damagePercentIncrease;
		typedOldResource._rangeIncrease += _rangeIncrease;
		typedOldResource._attackSpeedIncrease += _attackSpeedIncrease;
		typedOldResource._attackDelayModifierIncrease += _attackDelayModifierIncrease;
		typedOldResource._pierceCount += _pierceCount;

		return false;
	}

	public void RemoveFromOld(EffectResource oldResource)
	{
		StatsIncreaseResource typedOldResource = (StatsIncreaseResource)oldResource;

		typedOldResource._damageIncrease -= _damageIncrease;
		typedOldResource._damagePercentIncrease -= _damagePercentIncrease;
		typedOldResource._rangeIncrease -= _rangeIncrease;
		typedOldResource._attackSpeedIncrease -= _attackSpeedIncrease;
		typedOldResource._attackDelayModifierIncrease -= _attackDelayModifierIncrease;
		typedOldResource._pierceCount -= _pierceCount;
	}

	public override void SetDescription()
	{
		_effectDescription = "";
		if (_damageIncrease != 0)
		{
			_effectDescription += "Increase damage by " + _damageIncrease + "\n";
		}

		if (_damagePercentIncrease != 0)
		{
			_effectDescription += $"Increase damage by {_damagePercentIncrease * 100:F2}%\n";
		}


		if (_rangeIncrease != 0)
		{

			_effectDescription += "Increase range by " + _rangeIncrease + "\n";
		}

		if (_speedIncrease != 0)
		{

			_effectDescription += "Increase speed by " + _speedIncrease + "\n";
		}

		if (_attackSpeedIncrease != 0)
		{

			_effectDescription += "Increase attack speed by " + _attackSpeedIncrease * 100 + "%\n";
		}

		if (_attackDelayModifierIncrease != 0)
		{
			_effectDescription += "Increase retargeting speed by " + _attackDelayModifierIncrease * 100 + "%\n";
		}

		if (_disableDelay)
		{
			_effectDescription += "Retargeting becomes instant\n";
		}

		if (_pierceCount != 0)
		{
			_effectDescription += "Increase pierce count by " + _pierceCount + "\n";
		}

		if (_newZone is not null)
		{
			_effectDescription += "Changes attack area \n";
		}
	}

	public override void SetUpgradeDescription()
	{
		SetDescription();
	}

	public override Effect CreateNode()
	{
		return new StatsIncrease(this);
	}
}
