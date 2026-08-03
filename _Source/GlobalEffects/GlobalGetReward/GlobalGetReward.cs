using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;
public partial class GlobalGetReward : GlobalEffect
{
	private GlobalGetRewardResource _resource;

	public GlobalGetReward(GlobalGetRewardResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void OnCreation()
	{
		foreach (RewardManager.RewardType reward in _resource._rewards)
		{
			_tdManager.AddRewardNow(reward);
		}
	}
}