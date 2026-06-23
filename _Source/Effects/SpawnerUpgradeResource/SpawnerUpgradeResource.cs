using Godot;
using Godot.Collections;
using System.Data;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class SpawnerUpgradeResource : EffectResource
{
	[Export]
	public Array<InvaderStatsIncreaseResource> _units = new Array<InvaderStatsIncreaseResource>();

	[Export]
	public bool _applySameUpgradeForAllUnits = true;

	public override Effect CreateNode()
	{
		return new SpawnerUpgrade(this);
	}

	public override void SetDescription()
	{
		if (_effectDescription == "" && _applySameUpgradeForAllUnits)
		{
			_effectDescription = "Every spawned unit drops +" + Utils.MakeMoneyText(_units[0]._moneyBuff) + " but becomes stronger.";
		}
	}
}
