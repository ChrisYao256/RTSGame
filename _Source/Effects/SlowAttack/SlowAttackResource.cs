using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class SlowAttackResource : EffectResource
{
	[Export]
	public float _percentDecrease;

	[Export]
	public float _time;

	private SlowAttack _effect;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		SlowAttackResource typedOldResource = (SlowAttackResource)oldResource;
		//if (typedOldResource._change > _change && typedOldResource._effect._timer.TimeLeft < _time) // if new debuff is stronger and lasts longer, then remove old debuff and add new one.
		//{
		//	typedOldResource._effect.RemoveEffectResource();
		//	typedOldResource._effect.RemoveEffectNode();
		//	return true;
		//}
		//else if (typedOldResource._change <= _change && typedOldResource._effect._timer.TimeLeft < _time) // if new debuff is weaker but lasts longer, then .
		//{

		//	return true;
		//}
		//else if (typedOldResource._change > _change && typedOldResource._effect._timer.TimeLeft >= _time) // if new debuff is stronger but lasts shorter, then make this effect the difference and add it.
		//{
		//	_change = _change - typedOldResource._change;
		//	return true;
		//}
		//else // if new debuff is stronger but lasts shorter, then don't add new debuff.
		//{
		//	return false;
		//}

		typedOldResource._effect.AddResource(this);
		return false;
	}

	public override void SetDescription()
	{
		_effectName = "Slowed";
		_effectDescription = "Attack speed slowed by " + Math.Truncate(_percentDecrease * 100) + "%";
	}

	public override Effect CreateNode()
	{
		_effect = new SlowAttack(this);
		return _effect;
	}
}
