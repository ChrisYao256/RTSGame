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
	public float _percent;

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
			typedOldResource._percent += _percent;
			typedOldResource._time += _time;
			typedOldResource.SetDescription();
		}
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Small;
		
		if (_percent == 0)
		{
			_effectName = "Maimed " + (_damage).ToString("F0");
			_effectDescription = "Taking " + _damage.ToString() + " damage every second";
		}
		else
		{
			_effectName = $"Maimed {_percent* 100 :F0}%";
			_effectDescription = $"Taking {_percent* 100 :F0}% max HP damage every second";
		}
		_effectTopRightString = _time + "::duration::";
	}

	public override Effect CreateNode()
	{
		_effect = new DamageOverTime(this);
		return _effect;
	}
}
