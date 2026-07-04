using Godot;
using RTSGame._Source.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class DamageReductionResource : EffectResource, IStackable
{
	[Export]
	public float _percentDecrease;

	[Export]
	public float _time;

	public EffectResource MultiplyEffect(int n)
	{
		DamageReductionResource newResource = (DamageReductionResource)Duplicate();
		newResource._percentDecrease *= n;
		newResource.SetDescription();
		return newResource;
	}

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		DamageReductionResource typedOldResource = (DamageReductionResource)oldResource;

		if (typedOldResource._effect is not null)
		{
			((DamageReduction)typedOldResource._effect).AddResource(this);
		}
		else
		{
			typedOldResource._percentDecrease += _percentDecrease;
			typedOldResource._time += _time;
			typedOldResource.SetDescription();
		}
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Small;
		if (_effectName == "")
		{
			_effectName = "Protected";
		}
		_effectDescription = "Taking " + Math.Truncate(_percentDecrease * 100) + "% less damage";
		if (_time != -1)
		{
			_effectTopRightString = _time + "::duration::";
		}
		else
		{
			_effectTopRightString = "∞::duration::";
		}
	}

	public override void SetUpgradeDescription()
	{
		if (_effectName == "")
		{
			_effectName = "Protected";
		}
		_effectDescription = "";
		if (_percentDecrease != 0)
		{
			_effectDescription += $"{_effectName} strength +" + Math.Truncate(_percentDecrease * 100) + "%";
		}
		if (_time != 0)
		{
			_effectDescription += $"{_effectName} duration +" + _time + "::duration::";
		}
	}

	public override Effect CreateNode()
	{
		return new DamageReduction(this);
	}
}
