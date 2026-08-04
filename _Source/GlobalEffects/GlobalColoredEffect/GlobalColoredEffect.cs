using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;
public partial class GlobalColoredEffect : GlobalEffect
{
	private GlobalColoredEffectResource _resource;

	public GlobalColoredEffect(GlobalColoredEffectResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void OnCreation()
	{
		foreach (TowerUnit towerUnit in _tdManager._towerManager._towersOnField)
		{
			if (towerUnit is Spawner || !towerUnit._colors.Contains(_resource._color))
			{
				continue;
			}
			towerUnit.AddEffect((EffectResource)_resource._resource.Duplicate());
		}
	}

	protected override void OnPlacedTower(TowerUnit tower)
	{
		if (tower is Spawner || !tower._colors.Contains(_resource._color))
		{
			return;
		}
		tower.AddEffect((EffectResource)_resource._resource.Duplicate());
	}
}