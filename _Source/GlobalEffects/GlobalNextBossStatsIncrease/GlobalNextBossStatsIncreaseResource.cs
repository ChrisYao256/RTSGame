using Godot;
using Godot.Collections;
using System.Data;
using System.Drawing;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalNextBossStatsIncreaseResource : GlobalEffectResource
{
	[Export]
	public InvaderStatsIncreaseResource _statsIncrease;

	public override GlobalEffect CreateNode()
	{
		return new GlobalNextBossStatsIncrease(this);
	}

	public override void SetDescription()
	{
		_effectDescription = $"Next boss gain: \n";
		_statsIncrease.SetDescription();
		_effectDescription += _statsIncrease._effectDescription;
	}
}
