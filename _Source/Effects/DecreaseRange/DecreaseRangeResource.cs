using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class DecreaseRangeResource : EffectResource
{
	[Export]
	public float _percentDecrease;

	[Export]
	public float _time;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		DecreaseRangeResource typedOldResource = (DecreaseRangeResource)oldResource;

		if (typedOldResource._effect is not null)
		{
			((DecreaseRange)typedOldResource._effect).AddResource(this);
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
			_effectName = "Blinded";
		}
		if (_effectDescription == "")
		{
			_effectDescription = "Range decreased by " + Math.Truncate(_percentDecrease * 100) + "%";
		}
		_effectTopRightString = _time + "::duration::";
	}

	public override void SetUpgradeDescription()
	{
		_effectName = "Blinded";
		_effectDescription = "";
		if (_percentDecrease > 0)
		{
			_effectDescription += $"{_effectName} range decreased by {Math.Truncate(_percentDecrease * 100)}% more";
		}
		if (_time > 0)
		{
			_effectDescription += $"{_effectName} lasts {_time}::duration:: longer";
		}

	}

	public override Effect CreateNode()
	{
		_effect = new DecreaseRange(this);
		return _effect;
	}
}
