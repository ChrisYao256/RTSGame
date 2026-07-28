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
public partial class GlobalWeaponTypeStatsIncrease : GlobalEffect
{
	GlobalWeaponTypeStatsIncreaseResource _resource;

	public GlobalWeaponTypeStatsIncrease(GlobalWeaponTypeStatsIncreaseResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void OnCreation()
	{
		foreach (TowerUnit towerUnit in _tdManager._towerManager._allTowers)
		{
			if (towerUnit is Spawner)
			{
				continue;
			}
			if (towerUnit._weapon._weaponType == (_resource._weaponType))
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
		if (tower._weapon._weaponType == (_resource._weaponType))
		{
			tower.AddTowerStatsIncrease(_resource._statsIncrease);
			tower.EmitSignal(Unit.SignalName.UpdateStatsInfo);
		}
	}
}
