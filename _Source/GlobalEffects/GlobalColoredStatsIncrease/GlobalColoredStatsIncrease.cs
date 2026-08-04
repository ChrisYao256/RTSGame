using Godot;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

/// <summary>
/// Should be obsolete, since every unit has a StatsIncrease _data in its _effects.
/// </summary>
public partial class GlobalColoredStatsIncrease : GlobalEffect
{
	GlobalColoredStatsIncreaseResource _resource;

	public GlobalColoredStatsIncrease(GlobalColoredStatsIncreaseResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void OnCreation()
	{
		foreach (TowerUnit towerUnit in _tdManager._towerManager._towersOnField)
		{
			if (towerUnit is Spawner)
			{
				continue;
			}
			if (towerUnit._colors.Contains(_resource._color))
			{
				towerUnit.AddTowerStatsIncrease(_resource._statsIncrease);
				towerUnit.EmitSignal(Unit.SignalName.UpdateStatsInfo);
			}
		}
	}

	protected override void OnPlacedTower(TowerUnit tower)
	{
		if (tower is Spawner)
		{
			return;
		}
		if (tower._colors.Contains(_resource._color))
		{
			tower.AddTowerStatsIncrease(_resource._statsIncrease);
			tower.EmitSignal(Unit.SignalName.UpdateStatsInfo);
		}
	}
}
