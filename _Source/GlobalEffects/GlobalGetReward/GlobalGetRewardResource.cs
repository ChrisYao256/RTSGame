using Godot;
using Godot.Collections;
using RTSGame.Source;
using System.Data;
using System.Drawing;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalGetRewardResource : GlobalEffectResource
{
	[Export]
	public Array<RewardManager.RewardType> _rewards;

	public override GlobalEffect CreateNode()
	{
		return new GlobalGetReward(this);
	}

	public override void SetDescription()
	{
		
		foreach (RewardManager.RewardType reward in _rewards)
		{
			switch (reward)
			{
				case RewardManager.RewardType.Tower:
					_effectDescription += StringDB.Entries["TowerChoice"];
					break;
				case RewardManager.RewardType.Defense:
					_effectDescription += StringDB.Entries["DefenseChoice"];
					break;
				case RewardManager.RewardType.Portal:
					_effectDescription += StringDB.Entries["PortalChoice"];
					break;
				case RewardManager.RewardType.Passive:
					_effectDescription += StringDB.Entries["PassiveChoice"];
					break;
				case RewardManager.RewardType.TowerAll:
					_effectDescription += StringDB.Entries["TowerAnyChoice"];
					break;
			}
			if (_rewards.IndexOf(reward) != _rewards.Count - 1)
			{
				_effectDescription += "\n";
			}
		}

	}
}
