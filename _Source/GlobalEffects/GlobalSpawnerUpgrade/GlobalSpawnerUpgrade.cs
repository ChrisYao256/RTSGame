using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;
public partial class GlobalSpawnerUpgrade : GlobalEffect
{
	private GlobalSpawnerUpgradeResource _resource;

	public GlobalSpawnerUpgrade(GlobalSpawnerUpgradeResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void OnCreation()
	{
		foreach (TowerUnit towerUnit in _tdManager._towerManager._allTowers)
		{
			if (towerUnit is not Spawner)
			{
				continue;
			}
			Spawner spawner = (Spawner)towerUnit;
			for (int i = 0; i < spawner._spawnerData._units.Count; i++)
			{
				spawner.AddSpawnerUnitStatsIncrease(i, _resource._statsIncrease);
			}

		}
	}

	protected override void OnPlacedTower(TowerUnit tower)
	{
		if (tower is not Spawner)
		{
			return;
		}
		Spawner spawner = (Spawner)tower;
		for (int i = 0; i < spawner._spawnerData._units.Count; i++)
		{
			spawner.AddSpawnerUnitStatsIncrease(i, _resource._statsIncrease);
		}
	}
}