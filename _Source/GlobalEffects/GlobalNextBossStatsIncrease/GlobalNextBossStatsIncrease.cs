using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;
public partial class GlobalNextBossStatsIncrease : GlobalEffect
{
	private GlobalNextBossStatsIncreaseResource _resource;

	public GlobalNextBossStatsIncrease(GlobalNextBossStatsIncreaseResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void OnCreation()
	{
		_tdManager.AddMiniBossBuff(_resource._statsIncrease);
	}

	protected override void OnBossSpawned()
	{
		_tdManager.AddMiniBossBuff(_resource._statsIncrease.MultiplyEffect(-1));
	}
}