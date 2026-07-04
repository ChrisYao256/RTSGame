using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class DamageOverTimeResource : EffectResource
{
	[Export]
	public int _damage;

	[Export]
	public float _time;

	[Export]
	public PackedScene _burnVisualScene;

	private DamageOverTime _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		DamageOverTimeResource typedOldResource = (DamageOverTimeResource)oldResource;

		if (typedOldResource._effect is not null)
		{
			typedOldResource._effect.AddResource(this);
		}
		else
		{
			typedOldResource._damage += _damage;
			typedOldResource._time += _time;
			typedOldResource.SetDescription();
		}
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Small;
		_effectName = "Burning";
		_effectDescription = "Taking " + _damage.ToString() + " every second";
		_effectTopRightString = _time + "::duration::";
	}

	public override void SetUpgradeDescription()
	{
		_effectName = "Burning";
		_effectDescription = "";
		if (_damage != 0)
		{
			_effectDescription += $"{_effectName} damage +{_damage}";
		}
		if (_time != 0)
		{
			_effectDescription += $"{_effectName} duration +{_time}::duration::";
		}
	}

	public override Effect CreateNode()
	{
		_effect = new DamageOverTime(this);
		return _effect;
	}
}
