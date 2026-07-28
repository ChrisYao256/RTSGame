using Godot;
using RTSGame._Source.Units;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalStrongerSlowResource : GlobalEffectResource
{
	[Export]
	public float _extraSlow;


	public override void SetDescription()
	{
		_effectDescription = $"Slow effects are {_extraSlow * 100:F0}% stronger";
	}

	public override GlobalEffect CreateNode()
	{
		return new GlobalStrongerSlow(this);
	}
}
