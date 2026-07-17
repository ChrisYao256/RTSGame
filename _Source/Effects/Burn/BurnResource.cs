using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class BurnResource : EffectResource
{
	[Export]
	public int _damage;

	[Export]
	public PackedScene _burnVisualScene;

	private Burn _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		BurnResource typedOldResource = (BurnResource)oldResource;

		if (typedOldResource._effect is not null)
		{
			typedOldResource._effect.AddResource(this);
		}
		else
		{
			typedOldResource._damage += _damage;
			typedOldResource.SetDescription();
		}
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Small;
		_effectName = "Burning " + (_damage).ToString("F0");
		_effectDescription = "Taking " + _damage.ToString() + " every second";
	}

	public override void SetUpgradeDescription()
	{
		_effectName = "Burning";
		_effectDescription = "";
		if (_damage != 0)
		{
			_effectDescription += $"{_effectName} damage +{_damage}";
		}
	}

	public override Effect CreateNode()
	{
		_effect = new Burn(this);
		return _effect;
	}
}
