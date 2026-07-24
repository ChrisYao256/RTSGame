using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static Godot.Control;

namespace RTSGame.Source;

public partial class RogueManager : Node
{
	public static Array<int> _wavesWithTowerUnlock = [4, 9, 14, 19, 24];

	private TDManager _tdManager;

	private CanvasLayer _choicesLayer;
	private PanelContainer _choicesPanel;
	private TooltipRichTextLabel _choicesTitle;
	private HBoxContainer _choicesContainer;

	public Array<string> _remainingTowers;

	/// <summary>
	/// set to true if this the game was continued from a save. The initial choices won't appear
	/// </summary>
	public bool _loaded;

	private Array<TowerUnit.TowerType> _choicesQueue;
	
	public RogueManager(TDManager tdManager)
	{
		_tdManager = tdManager;
	}

	public override void _Ready()
	{
		_remainingTowers = _tdManager._allTowerList;
		InitializeRogueChoicePrompt();
		if (!_loaded)
		{
			_choicesQueue = [TowerUnit.TowerType.Defense, TowerUnit.TowerType.Spawner, TowerUnit.TowerType.Null, TowerUnit.TowerType.Null];
			ProcessChoicesQueue();
		}
		else
		{
			_choicesQueue = [];
			foreach (string tower in _tdManager._availTowerList)
			{
				_remainingTowers.Remove(tower);
			}
		}
	}

	public void InitializeRogueChoicePrompt()
	{
		_choicesPanel = new PanelContainer();
		VBoxContainer vboxContainer = new VBoxContainer();

		_choicesTitle = new TooltipRichTextLabel();
		_choicesTitle.FitContent = true;
		_choicesTitle.CustomMinimumSize = new Vector2(0, 50);
		_choicesTitle.HorizontalAlignment = HorizontalAlignment.Center;
		vboxContainer.AddChild(_choicesTitle);

		_choicesContainer = new HBoxContainer();
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
		foreach (var child in _choicesContainer.GetChildren())
		{
			child.QueueFree();
		}
		_choicesTitle.Text = StringDB.Entries["TowerChoice"];
		foreach (string name in towers)
		{
			VBoxContainer container = new VBoxContainer();

			string name_ = name;
			TowerUnit unit = (TowerUnit)UnitManager.GetUnit(name, true);

			Label nameLabel = new Label();
			nameLabel.Text = unit._name;
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

	public void OnNewWave(int count)
	{
		if (_wavesWithTowerUnlock.Contains(count))
		{
			_choicesQueue.Add(TowerUnit.TowerType.Null);
			ProcessChoicesQueue();
		}
	}

	private void UnlockTower(string name)
	{
		_tdManager.UnlockTower(name);
		_remainingTowers.Remove(name);
		TowerUnit tower = (TowerUnit)UnitManager.GetUnit(name, false);
		_tdManager.GainMoney(tower._cost);
		tower.QueueFree();
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

	private void ProcessChoicesQueue()
	{
		if (_choicesQueue.Count == 0)
		{
			return;
		}
		MakeRogueTowerUnlockChoicePrompt(GetRandomTowers(3, _choicesQueue[0]));
		_choicesQueue.RemoveAt(0);
	}
}