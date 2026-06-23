using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class DropMoreMoneyResource : EffectResource
{
	[Export]
	public Vector4I _increase;

	[Export]
	public float _time;

	private DropMoreMoney _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		DropMoreMoneyResource typedOldResource = (DropMoreMoneyResource)oldResource;

		typedOldResource._effect.AddResource(this);
		return false;
	}

	public override void SetDescription()
	{
		_effectName = "Marked";
		_displayType = DisplayTypes.Small;
		_effectDescription = "Drops " + Utils.MakeMoneyText(_increase) + " more resources.";
		_effectTopRightString = _time + "::duration::";
	}

	public override Effect CreateNode()
	{
		_effect = new DropMoreMoney(this);
		return _effect;
	}
}
