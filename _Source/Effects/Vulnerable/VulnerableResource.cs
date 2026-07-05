using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class VulnerableResource : EffectResource
{
	[Export]
	public float _percentIncrease;

	[Export]
	public float _time;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		VulnerableResource typedOldResource = (VulnerableResource)oldResource;

		if (typedOldResource._effect is not null)
		{
			((Vulnerable)typedOldResource._effect).AddResource(this);
		}
		else
		{
			typedOldResource._percentIncrease += _percentIncrease;
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
			_effectName = "Vulnerable " + (_percentIncrease*100).ToString("F0");
		}
		_effectDescription = "Taking " + Math.Truncate(_percentIncrease * 100) + "% more damage";
		_effectTopRightString = _time + "::duration::";
	}

	public override void SetUpgradeDescription()
	{
		if (_effectName == "")
		{
			_effectName = "Slowed";
		}
		_effectDescription = "";
		if (_percentIncrease != 0)
		{
			_effectDescription += $"{_effectName} strength +" + Math.Truncate(_percentIncrease * 100) + "%";
		}
		if (_time != 0)
		{
			_effectDescription += $"{_effectName} duration +" + _time + "::duration::";
		}
	}

	public override Effect CreateNode()
	{
		return new Vulnerable(this);
	}
}
