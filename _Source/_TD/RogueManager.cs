using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using static Godot.Control;

namespace RTSGame.Source;

public partial class RogueManager : Node
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

	//public static Array<int> _wavesWithTowerUnlock = [4, 9, 14, 19, 24];
	//public static Array<int> _wavesWithPassive = [2, 7, 12, 17, 22, 27];

	public Array<GlobalEffectResource> _remainingEffects;

	private TDManager _tdManager;

	private CanvasLayer _choicesLayer;
	private PanelContainer _choicesPanel;
	private TooltipRichTextLabel _choicesTitle;
	private GridContainer _choicesContainer;

	public Array<string> _remainingTowers;

	/// <summary>
	/// set to true if this the game was continued from a save. The initial choices won't appear
	/// </summary>
	public bool _noStartingReward;

	public Array<RewardType> _choicesQueue;
	
	public RogueManager(TDManager tdManager)
	{
		_tdManager = tdManager;
	}

	public override void _Ready()
	{
		_remainingTowers = _tdManager._allTowerList.Duplicate();
		_remainingEffects = _tdManager._allGlobalEffects.Duplicate();
		InitializeRogueChoicePrompt();
		if (!_noStartingReward)
		{
			_choicesQueue = [RewardType.Passive, RewardType.Defense, RewardType.Portal, RewardType.Defense, RewardType.Portal];
			ProcessChoicesQueue();
		}
		else
		{
			_choicesQueue = [];
			foreach (string tower in _tdManager._availTowerList)
			{
				_remainingTowers.Remove(tower);
			}
			foreach (GlobalEffectResource resource in _tdManager._globalEffects)
			{
				_remainingEffects.Remove(resource);
			}
		}
	}

	public void InitializeRogueChoicePrompt()
	{
		_choicesPanel = new PanelContainer();
		VBoxContainer vboxContainer = new VBoxContainer();

		_choicesTitle = new TooltipRichTextLabel();
		_choicesTitle.FitContent = true;
		_choicesTitle.CustomMinimumSize = new Vector2(300, 0);
		_choicesTitle.HorizontalAlignment = HorizontalAlignment.Center;
		vboxContainer.AddChild(_choicesTitle);

		_choicesContainer = new GridContainer();
		_choicesContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		_choicesContainer.AddThemeConstantOverride("h_separation", 32);
		_choicesContainer.AddThemeConstantOverride("v_separation", 32);
		//_choicesContainer.Alignment = BoxContainer.AlignmentMode.Center;
		vboxContainer.AddChild(_choicesContainer);
		

		_choicesPanel.AddChild(vboxContainer);
		_choicesPanel.SetAnchorsPreset(LayoutPreset.Center);
		_choicesPanel.GrowHorizontal = GrowDirection.Both;
		_choicesPanel.GrowVertical = GrowDirection.Both;
		_choicesLayer = GetParent().GetNode<CanvasLayer>("RogueLayer");
		_choicesLayer.AddChild(_choicesPanel);
	}

	public void MakeRogueTowerUnlockChoicePrompt(Array<string>towers)
	{
		if (towers.Count == 0)
		{
			if (_choicesQueue.Count > 0)
			{
				ProcessChoicesQueue();
				return;
			}
		}
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
				UnlockTower(name_);
				_choicesLayer.Hide();
				if (_choicesQueue.Count > 0)
				{
					ProcessChoicesQueue();
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
				costLabel.Text = Utils.MakeMoneyText(unit._cost);
			}
			else
			{
				if (unit.GetIncome() != new Vector4I(0,0,0,0))
				{
					costLabel.Text = "+" + Utils.MakeMoneyText(unit.GetIncome());
				}
			}
			costLabel.HorizontalAlignment = HorizontalAlignment.Center;
			container.AddChild(costLabel);

			_choicesContainer.AddChild(container);

			unit.QueueFree();
		}
		_choicesLayer.Show();
	}

	public void MakeRoguePassiveUnlockChoicePrompt(Array<GlobalEffectResource> resources)
	{
		if (resources.Count == 0)
		{
			if (_choicesQueue.Count > 0)
			{
				ProcessChoicesQueue();
				return;
			}
		}
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
				_choicesLayer.Hide();
				int test = _tdManager._allGlobalEffects.IndexOf(resource);
				GivePassive(resource);
				if (_choicesQueue.Count > 0)
				{
					ProcessChoicesQueue();
				}
			});

			container.AddChild(effectButton);

			_choicesContainer.AddChild(container);
		}
		_choicesLayer.Show();
	}

	private void UnlockTower(string name)
	{
		_tdManager.UnlockTower(name);
		_remainingTowers.Remove(name);
		TowerUnit tower = (TowerUnit)UnitManager.GetUnit(name, false);
		_tdManager.GainMoney(tower._cost);
		tower.QueueFree();
	}

	private void GivePassive(GlobalEffectResource resource)
	{
		int test = _tdManager._allGlobalEffects.IndexOf(resource);
		_remainingEffects.Remove(resource);
		_tdManager.ApplyGlobalEffect(resource);
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

	public void ProcessChoicesQueue()
	{
		if (_choicesQueue.Count == 0)
		{
			return;
		}
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