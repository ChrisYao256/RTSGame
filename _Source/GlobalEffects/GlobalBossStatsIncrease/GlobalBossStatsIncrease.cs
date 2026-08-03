using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;
public partial class GlobalBossStatsIncrease : GlobalEffect
{
	private GlobalBossStatsIncreaseResource _resource;

	public GlobalBossStatsIncrease(GlobalBossStatsIncreaseResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void OnCreation()
	{
		if (_resource._forMiniBoss)
		{
			_tdManager.AddMiniBossBuff(_resource._statsIncrease);
		}
		else
		{
			_tdManager.AddFinalBossBuff(_resource._statsIncrease);
		}
	}
}