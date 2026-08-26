using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;

namespace RTSGame.Units;

public partial class EventTower : TowerUnit
{
	[Export]
	public Event _event;

	public override void _Ready()
	{
		base._Ready();
		_sellable = false;
		_movable = false;
		_removable = false;
	}

	public override Godot.Collections.Dictionary<string, PanelContainer> MakeUnitInfoContainer()
	{
		base.MakeUnitInfoContainer();

		PanelContainer eventInfo = new();
		HBoxContainer eventInfoH = new();

		_infoContainers.Add("Event", eventInfo);
		eventInfo.AddChild(eventInfoH);

		VBoxContainer vbox = new();
		eventInfoH.AddChild(vbox);

		TooltipRichTextLabel rtl = new();
		rtl.Text = _event._description;
		rtl.CustomMinimumSize = new(200, 0);
		rtl.FitContent = true;
		vbox.AddChild(rtl);

		if (_event is BossEvent bossEvent)
		{
			Button button = new();
			vbox.AddChild(button);

			button.Pressed += async () =>
			{
				if (_tdManager.CheckWaveFinished())
				{
					_tdManager._currentWaveRewards = bossEvent._bossReward;
					Array<InvaderStatsIncreaseResource> challengeUnitsCopy = bossEvent._boss._units.Duplicate();
					_tdManager._leakedInvaderCount = 0;
					_tdManager._waveEndProcessed = false;
					_tdManager._towerManager.RemoveTower(_gridLocation);
					await _tdManager.SpawnMiniBossWave(challengeUnitsCopy, 0);
				}

			};

			button.Text = "Activate";
			
		}
		else if (_event is RewardEvent rewardEvent)
		{
			Button button = new();
			vbox.AddChild(button);

			button.Pressed += () =>
			{
				if (_tdManager.CheckWaveFinished())
				{
					_tdManager._rewardManager._choicesQueue.AddRange(rewardEvent._reward);
					_tdManager._rewardManager.MakeRewardPrompt(RewardManager.RewardSource.Event);
					_tdManager._towerManager.RemoveTower(_gridLocation);
				}
			};

			button.Text = "Claim";
		}
		else if (_event is PayEvent payEvent)
		{
			Button button = new();
			vbox.AddChild(button);

			button.Pressed += () =>
			{
				if (_tdManager.CheckWaveFinished())
				{
					if (Utils.VectorLeq(payEvent._price, _tdManager._money) && _tdManager._hp > payEvent._hpPrice)
					{
						_tdManager.SpendMoney(payEvent._price);
						_tdManager.IncreaseHp(-payEvent._hpPrice);
						_tdManager._rewardManager._choicesQueue.AddRange(payEvent._reward);
						_tdManager._rewardManager.MakeRewardPrompt(RewardManager.RewardSource.Event);
						_tdManager._towerManager.RemoveTower(_gridLocation);
					}
				}
			};

			button.Text = "Buy";
		}

		return _infoContainers;
	}
}