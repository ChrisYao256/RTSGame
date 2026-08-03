using Godot;
using Godot.Collections;
using System.Data;
using System.Drawing;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalBossStatsIncreaseResource : GlobalEffectResource
{
	[Export]
	public InvaderStatsIncreaseResource _statsIncrease;

	/// <summary>
	/// only applies to final boss if set to false. Otherwise only applies to mini bosses.
	/// </summary>
	[Export]
	public bool _forMiniBoss;

	public override GlobalEffect CreateNode()
	{
		return new GlobalBossStatsIncrease(this);
	}

	public override void SetDescription()
	{
		_effectDescription = $"All bosses gain: \n";
		_statsIncrease.SetDescription();
		_effectDescription += _statsIncrease._effectDescription;
	}
}
