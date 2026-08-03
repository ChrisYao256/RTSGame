using Godot;
using RTSGame._Source.Units;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalSlowDamageResource : GlobalEffectResource
{
	[Export]
	public float _percentDamage;


	public override void SetDescription()
	{
		_effectDescription = $"Slowed enemies take {_percentDamage * 100:F0}% max HP every second.";
	}

	public override GlobalEffect CreateNode()
	{
		return new GlobalSlowDamage(this);
	}
}
