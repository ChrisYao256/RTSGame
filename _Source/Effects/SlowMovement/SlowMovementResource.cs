using Godot;
using System;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class SlowMovementResource : EffectResource
{
	[Export]
	public float _percentDecrease;

	[Export]
	public float _time;

	private SlowMovement _effect;

	public override bool MergeWithOld(EffectResource oldResource)
	{
		SlowMovementResource typedOldResource = (SlowMovementResource)oldResource;

		typedOldResource._effect.AddResource(this);
		return false;
	}

	public override void SetDescription()
	{
		_effectName = "Slowed";
		_effectDescription = "Move speed slowed by " + Math.Truncate(_percentDecrease * 100) + "%";
	}

	public override Effect CreateNode()
	{
		_effect = new SlowMovement(this);
		return _effect;
	}
}
