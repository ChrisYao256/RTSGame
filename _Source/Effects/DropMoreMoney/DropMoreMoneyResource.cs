using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class DropMoreMoneyResource : EffectResource
{
	[Export]
	public float _percentIncrease;

	[Export]
	public float _time;

	private DropMoreMoney _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		DropMoreMoneyResource typedOldResource = (DropMoreMoneyResource)oldResource;

		if (typedOldResource._effect is not null)
		{
			typedOldResource._effect.AddResource(this);
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
		_effectName = "Analyzed";
		_displayType = DisplayTypes.Small;
		_effectDescription = $"Drops " + Math.Truncate(_percentIncrease * 100) + "% more resources.";
		_effectTopRightString = _time + "::duration::";
	}

	public override void SetUpgradeDescription()
	{
		_effectName = "Analyzed";
		_effectDescription = "";
		if (_percentIncrease > 0)
		{
			_effectDescription += $"{_effectName} units drop " + (_percentIncrease * 100).ToString("F2") + "% more resources.\n";
		}
		if (_time > 0)
		{
			_effectDescription += $"{_effectName} lasts +{_time}::duration::.\n";
		}
		_effectDescription.Trim();
	}

	public override Effect CreateNode()
	{
		_effect = new DropMoreMoney(this);
		return _effect;
	}
}
