using Godot;
using RTSGame._Source.Units;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalStatsIncreaseResource : GlobalEffectResource
{
	[Export]
	public StatsIncreaseResource _statsIncrease;


	public override void SetDescription()
	{
		_effectDescription = "All towers gain: \n";
		_statsIncrease.SetDescription();
		_effectDescription += _statsIncrease._effectDescription;
	}

	public override GlobalEffect CreateNode()
	{
		return new GlobalStatsIncrease(this);
	}
}
