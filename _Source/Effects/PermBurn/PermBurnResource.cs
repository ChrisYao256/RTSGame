using Godot;
using RTSGame._Source.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class PermBurnResource : EffectResource
{

	[Export]
	public float _time;

	PermBurn _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{


		PermBurnResource typedOldResource = (PermBurnResource)oldResource;

		if (typedOldResource._effect is not null)
		{
			typedOldResource._effect.AddResource(this);
		}
		else
		{
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
			_effectName = "Chemical Burn";
		}
		_effectDescription = "Burn on this unit does not decrease";
		if (_time != -1)
		{
			_effectTopRightString = _time + "::duration::";
		}
		else
		{
			_effectTopRightString = "∞::duration::";
		}
	}

	public override Effect CreateNode()
	{
		_effect = new PermBurn(this);
		return _effect;
	}
}
