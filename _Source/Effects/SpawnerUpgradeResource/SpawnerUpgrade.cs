using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;
public partial class SpawnerUpgrade : Effect
{
	private SpawnerUpgradeResource _resource;

	public SpawnerUpgrade(SpawnerUpgradeResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
	}

	protected override void OnCreation()
	{
		if (_parentUnit is not Spawner)
		{
			throw new Exception("Parent is not an invader!");
		}
		Spawner spawner = (Spawner)_parentUnit;
		if (_resource._applySameUpgradeForAllUnits)
		{
			for (int i = 0; i < spawner._data._units.Count; i++)
			{
				spawner.AddSpawnerUnitStatsIncrease(i, _resource._units[0]);
			}
		}
		else
		{
			for (int i = 0; i < spawner._data._units.Count; i++)
			{
				spawner.AddSpawnerUnitStatsIncrease(i, _resource._units[i]);
			}
		}
		QueueFree();
	}
}