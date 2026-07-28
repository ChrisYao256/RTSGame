using Godot;
using Godot.Collections;
using System.Data;
using System.Drawing;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalSpawnerUpgradeResource : GlobalEffectResource
{
	[Export]
	public InvaderStatsIncreaseResource _statsIncrease;

	public override GlobalEffect CreateNode()
	{
		return new GlobalSpawnerUpgrade(this);
	}

	public override void SetDescription()
	{
		_effectDescription = $"All portals gain: \n";
		_statsIncrease.SetDescription();
		_effectDescription += _statsIncrease._effectDescription;
	}
}
