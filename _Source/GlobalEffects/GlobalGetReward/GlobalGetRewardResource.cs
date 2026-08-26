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
	public Array<RewardResource> _rewards;

	public override GlobalEffect CreateNode()
	{
		return new GlobalGetReward(this);
	}

	public override void SetDescription()
	{
		
		foreach (RewardResource reward in _rewards)
		{
			if (reward is TowerRewardResource towerResource)
			{
				switch (towerResource._type)
				{
					case TowerRewardResource.TowerType.Tower:
						_effectDescription += StringDB.Entries["TowerChoice"];
						break;
					case TowerRewardResource.TowerType.Defense:
						_effectDescription += StringDB.Entries["DefenseChoice"];
						break;
					case TowerRewardResource.TowerType.Portal:
						_effectDescription += StringDB.Entries["PortalChoice"];
						break;
					case TowerRewardResource.TowerType.TowerAll:
						_effectDescription += StringDB.Entries["TowerAnyChoice"];
						break;
				}
			}
			else if (reward is PassiveRewardResource passiveResource)
			{
				switch (passiveResource._type)
				{
					case PassiveRewardResource.PassiveType.FromThree:
						_effectDescription += StringDB.Entries["PassiveChoice"];
						break;
					case PassiveRewardResource.PassiveType.FromAll:
						_effectDescription += StringDB.Entries["PassiveAnyChoice"];
						break;
				}
			}

			if (_rewards.IndexOf(reward) != _rewards.Count - 1)
			{
				_effectDescription += "\n";
			}
		}

	}
}
