using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace RTSGame.Source;

public partial class RewardManager : CanvasLayer
{
	/// <summary>
	/// enums without All are chosen from 3, enums with all are chosen from all. 
	/// </summary>
	public enum RewardType
	{
		Defense,
		Portal,
		Tower,
		Passive,
		DefenseAll,
		PortalAll,
		TowerAll,
		PassiveAll,
	}

	public enum RewardSource
	{
		Starting,
		Boss,
		GlobalEffect
	}

	//public static Array<int> _wavesWithTowerUnlock = [4, 9, 14, 19, 24];
	//public static Array<int> _wavesWithPassive = [2, 7, 12, 17, 22, 27];

	public Array<GlobalEffectResource> _remainingEffects;

	private TDManager _tdManager;

	private PanelContainer _rewardsPanel;
	private Label _rewardsTitle;
	private PanelContainer _choicesPanel;
	private Label _choicesTitle;
	private GridContainer _choicesContainer;

	public Array<string> _remainingTowers;

	/// <summary>
	/// set to true if this the game was continued from a save. The initial choices won't appear
	/// </summary>
	public bool _noStartingReward;

	public Array<RewardType> _choicesQueue = [];

	private RewardSource _source;

	private bool _processingChoicesQueue = false;

	public override void _Ready()
	{
		_tdManager = GetParent().GetNode<TDManager>("TdManager");
		
		_rewardsPanel = GetNode<PanelContainer>("PanelContainer");
		_rewardsTitle = _rewardsPanel.GetNode<Label>("VBoxContainer/Label");
		_choicesPanel = GetNode<PanelContainer>("PanelContainer2");
		_choicesContainer = _choicesPanel.GetNode<GridContainer>("VBoxContainer/GridContainer");
		_choicesTitle = _choicesPanel.GetNode<Label>("VBoxContainer/Label");
	}

	public void Initialize()
	{
		_remainingTowers = _tdManager._allTowerList.Duplicate();
		_remainingEffects = _tdManager._allGlobalEffects.Duplicate();
		if (!_noStartingReward)
		{
			_choicesQueue = [RewardType.Passive, RewardType.Defense, RewardType.Portal, RewardType.Defense, RewardType.Portal];
			MakeRewardPrompt(RewardSource.Starting);
		}
		foreach (string tower in _tdManager._availTowerList)
		{
			_remainingTowers.Remove(tower);
		}
		foreach (GlobalEffectResource resource in _tdManager._globalEffects)
		{
			_remainingEffects.Remove(resource);
		}
	}

	public void MakeRogueTowerUnlockChoicePrompt(Array<string>towers)
	{
		if (towers.Count == 0)
		{
			_processingChoicesQueue = false;
			if (_choicesQueue.Count > 0)
			{
				MakeRewardPrompt(_source);
				return;
			}
			else
			{
				Hide();
				return;
			}
		}
		Show();
		_choicesPanel.Show();
		_rewardsPanel.Hide();
		foreach (var child in _choicesContainer.GetChildren())
		{
			child.QueueFree();
		}
		_choicesTitle.Text = StringDB.Entries["TowerChoice"];
		if (towers.Count < 6)
		{
			_choicesContainer.Columns = towers.Count;
		}
		else
		{
			_choicesContainer.Columns = 5;
		}
		foreach (string name in towers)
		{
			VBoxContainer container = new VBoxContainer();

				string name_ = name;
			TowerUnit unit = (TowerUnit)UnitManager.GetUnit(name, true);

			Label nameLabel = new Label();
			nameLabel.Text = unit.GetName();
			nameLabel.CustomMinimumSize = new(160, 0);
			nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
			container.AddChild(nameLabel);

			AddChild(unit);

			HoverInfoImage towerButton
				= unit.MakeTowerTooltip(true);
			towerButton.Pressed += (() =>
			{
				_processingChoicesQueue = false;
				UnlockTower(name_);
				Hide();
				if (_choicesQueue.Count > 0)
				{
					MakeRewardPrompt(_source);
				}
			});
			towerButton.MouseEntered += () => nameLabel.AddThemeColorOverride("font_color", ThemePalette.White);
			towerButton.MouseExited += () => nameLabel.AddThemeColorOverride("font_color", ThemePalette.Yellow);

			PanelContainer panelContainer = new();
			panelContainer.AddChild(towerButton);
			container.AddChild(panelContainer);

			TooltipRichTextLabel costLabel = new TooltipRichTextLabel();
			costLabel.FitContent = true;
			costLabel.BbcodeEnabled = true;
			if (unit is not Spawner)
			{
				costLabel.Text = Utils.MakeMoneyText(unit._cost, multiline: true);
			}
			else
			{
				if (unit.GetIncome() != new Vector4I(0, 0, 0, 0) || unit.GetUnknownIncome() > 0)
				{
					costLabel.Text = Utils.MakeMoneyText(unit.GetIncome(), multiline: true, unknownMoney: unit.GetUnknownIncome(), additionSigns: true);
				}
			}
			costLabel.HorizontalAlignment = HorizontalAlignment.Center;
			container.AddChild(costLabel);

			_choicesContainer.AddChild(container);

			unit.QueueFree();
		}
		Show();
	}

	public void MakeRoguePassiveUnlockChoicePrompt(Array<GlobalEffectResource> resources)
	{
		if (resources.Count == 0)
		{
			_processingChoicesQueue = false;
			if (_choicesQueue.Count > 0)
			{
				MakeRewardPrompt(_source);
				return;
			}
			else
			{
				return;
			}
		}
		Show();
		_choicesPanel.Show();
		_rewardsPanel.Hide();
		foreach (var child in _choicesContainer.GetChildren())
		{
			child.QueueFree();
		}
		_choicesTitle.Text = StringDB.Entries["PassiveChoice"];
		if (resources.Count < 6)
		{
			_choicesContainer.Columns = resources.Count;
		}
		else
		{
			_choicesContainer.Columns = 5;
		}
		foreach (GlobalEffectResource resource in resources)
		{
			VBoxContainer container = new VBoxContainer();

			resource.SetDescription();

			HoverInfoImage effectButton
				= resource.MakeEffectTooltip(true);
			effectButton.Pressed += (() =>
			{
				_processingChoicesQueue = false;
				Hide();
				int test = _tdManager._allGlobalEffects.IndexOf(resource);
				GivePassive(resource);
				if (_choicesQueue.Count > 0)
				{
					MakeRewardPrompt(_source);
				}
			});

			container.AddChild(effectButton);

			_choicesContainer.AddChild(container);
		}
		Show();
	}

	private void UnlockTower(string name)
	{
		_tdManager.UnlockTower(name);
		_remainingTowers.Remove(name);
	}

	private void GivePassive(GlobalEffectResource resource)
	{
		int test = _tdManager._allGlobalEffects.IndexOf(resource);
		_remainingEffects.Remove(resource);
		_tdManager.ApplyGlobalEffect(resource, false);
	}

	public Array<string> GetRandomTowers(int count, TowerUnit.TowerType type = TowerUnit.TowerType.Null)
	{
		Array<string> towers = [];
		foreach (string name in _remainingTowers)
		{
			TowerUnit tower = (TowerUnit)UnitManager.GetUnit(name, false);
			if (type == TowerUnit.TowerType.Null || tower._towerType == type)
			{
				towers.Add(tower._internalName);
			}
			tower.QueueFree();
		}
		return Utils.GetRandomElements<string>(towers, count);
	}

	public Array<GlobalEffectResource> GetRandomPassives(int count)
	{
		return Utils.GetRandomElements<GlobalEffectResource>(_remainingEffects, count);
	}

	public void MakeRewardPrompt(RewardSource source)
	{
		if (_choicesQueue.Count == 0)
		{
			return;
		}
		Show();
		_source = source;
		_choicesPanel.Hide();
		_rewardsPanel.Show();
		switch (source)
		{
			case RewardSource.Starting:
				_rewardsTitle.Text = "Claim your starting unlocks to begin the game:";
				break;
			case RewardSource.Boss:
				_rewardsTitle.Text = "Boss wave cleared! Claim your rewards to continue:";
				break;
			case RewardSource.GlobalEffect:
				_rewardsTitle.Text = "Claim your rewards:";
				break;
		}
		VBoxContainer vbox = _rewardsPanel.GetNode<VBoxContainer>("VBoxContainer/VBoxContainer");
		foreach (var node in vbox.GetChildren())
		{
			node.QueueFree();
		}
		foreach (RewardType type in _choicesQueue)
		{
			Button button = new Button();
			button.Pressed += () =>
			{
				_choicesQueue.Remove(type);
			};

			switch (type)
			{
				case RewardType.Defense:
					button.Text = "Unlock a Defense";
					button.Pressed += () =>
					{
						MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(3, TowerUnit.TowerType.Defense));
					};
					break;
				case RewardType.Portal:
					button.Text = "Unlock a Portal";
					button.Pressed += () =>
					{
						MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(3, TowerUnit.TowerType.Spawner));
					};
					break;
				case RewardType.Tower:
					button.Text = "Unlock a Defense or Portal";
					button.Pressed += () =>
					{
						MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(3, TowerUnit.TowerType.Null));
					};
					break;
				case RewardType.Passive:
					button.Text = "Get a Passive";
					button.Pressed += () =>
					{
						MakeRoguePassiveUnlockChoicePrompt(GetRandomPassives(3));
					};
					break;
				case RewardType.PassiveAll:
					button.Text = "Get any passive";
					button.Pressed += () =>
					{
						MakeRoguePassiveUnlockChoicePrompt(GetRandomPassives(_remainingEffects.Count));
					};
					break;
				case RewardType.TowerAll:
					button.Text = "Unlock any Defense";
					button.Pressed += () =>
					{
						MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(_remainingTowers.Count, TowerUnit.TowerType.Null));
					};
					break;
			}

			vbox.AddChild(button);
		}
	}

	public void ProcessChoicesQueue()
	{
		if (_processingChoicesQueue == true)
		{
			return;
		}
		if (_choicesQueue.Count == 0)
		{
			return;
		}
		_processingChoicesQueue = true;
		RewardType type = _choicesQueue[0];
		_choicesQueue.RemoveAt(0);
		switch (type)
		{
			case RewardType.Defense:
				MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(3, TowerUnit.TowerType.Defense));
				break;
			case RewardType.Portal:
				MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(3, TowerUnit.TowerType.Spawner));
				break;
			case RewardType.Tower:
				MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(3, TowerUnit.TowerType.Null));
				break;
			case RewardType.Passive:
				MakeRoguePassiveUnlockChoicePrompt(GetRandomPassives(3));
				break;
			case RewardType.DefenseAll:
				MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(3, TowerUnit.TowerType.Defense));
				break;
			case RewardType.PortalAll:
				MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(3, TowerUnit.TowerType.Spawner));
				break;
			case RewardType.TowerAll:
				MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(_remainingTowers.Count, TowerUnit.TowerType.Null));
				break;
			case RewardType.PassiveAll:
				MakeRoguePassiveUnlockChoicePrompt(GetRandomPassives(_remainingEffects.Count));
				break;
		}
		
	}
}