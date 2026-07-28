using Godot;
using RTSGame._Source.Units;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalFasterBurnResource : GlobalEffectResource
{
	[Export]
	public float _percentIncrease;


	public override void SetDescription()
	{
		_effectDescription = $"Burn decays rate reduced by -{_percentIncrease*100:F0}% ";
	}

	public override GlobalEffect CreateNode()
	{
		return new GlobalFasterBurn(this);
	}
}
