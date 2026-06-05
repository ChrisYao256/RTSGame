using Godot;
using RTSGame.Source;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static Godot.GodotThread;
using static System.Net.Mime.MediaTypeNames;

public partial class UnitInfoPanel : CanvasLayer
{
	private Label _nameLabel;
	private Label _hpLabel;
	private Label _shieldLabel;
	private Label _speedLabel;
	private Label _totalCostLabel;
	private Label _armorLabel;
	private Label _positionLabel;
	private VBoxContainer _weaponContainer;
	private Label _damageLabel;
	private Label _cooldownLabel;
	private Label _dpsLabel;
	private Label _rangeLabel;
	private Label _incomeLabel;
	private Label _moneyDroppedLabel;
	private Label _spawnLabel;

	private VBoxContainer _priorityContainer;
	private Label _priorityLabel;

	private HBoxContainer _effectsContainer;
	private HBoxContainer _infoContainer;
	private VBoxContainer _commandContainer;
	private HBoxContainer _upgradeContainer;
	private BoxContainer _unitsContainer;
	private HBoxContainer _hBoxContainer;
	private PanelContainer _container;

	private List<Unit> _units;
	private Unit _unit;

	private TDManager _tdManager;

	public override void _Ready()
	{
		// Link our C# variables to the nodes in the scene
		_nameLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/UnitNameLabel");
		_hpLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/Hp");
		_shieldLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/Shield");
		_speedLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/SpeedLabel");
		_totalCostLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/TotalCostLabel");
		_armorLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/ArmorLabel");
		_weaponContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2");
		_damageLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/DamageLabel");
		_cooldownLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/CooldownLabel");
		_rangeLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/RangeLabel");
		_dpsLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/DPSLabel");
		_incomeLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/IncomeLabel");
		_moneyDroppedLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/MoneyDroppedLabel");
		_spawnLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/SpawnLabel");

		_priorityContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/UnitInfo/TargetPriorityContainer");
		_priorityLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/TargetPriorityContainer/TargetPriority");
		_effectsContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer/UnitInfo/Effects");

		_positionLabel = GetNode<Label>("PanelContainer/HBoxContainer/Position");
		
		_infoContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer/UnitInfo");

		_upgradeContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer/Upgrades");

		_commandContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/CommandContainer");

		_unitsContainer = GetNode<BoxContainer>("PanelContainer/HBoxContainer/UnitsContainer");

		_hBoxContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer");
		_container = GetNode<PanelContainer>("PanelContainer");

		_tdManager = GetParent().GetNode<TDManager>("TdManager");
	}

	// This method will be called by your Selection Manager
	public void UpdateSelectedUnits(List<Unit> units)
	{
		HideUnitInfo();
		_units = units;
		if (_units is null)
		{
			return;
		}
		if (_units.Count == 1)
		{
			_unit = units[0];
			_unit.Connect(Unit.SignalName.UpdateInfo, Callable.From(UpdateAllUnitInfo));
			_unit.Connect(Unit.SignalName.UpdateStatsInfo, Callable.From(UpdateUnitStatsInfo));
			UpdateAllUnitInfo();
		}
		if (_units.Count > 1)
		{
			for (int i = _unitsContainer.GetChildCount() - 1; i >= 0; i--)
			{
				_unitsContainer.GetChild(i).Free();
			}
			Dictionary<string, int> unitTypes = new Dictionary<string, int>();
			bool sameTower = true;
			for (int i = 0; i < _units.Count; i++)
			{
				if (_units[i]._internalName != _units[0]._internalName || _units[i] is not TowerUnit)
				{
					sameTower = false;
				}
				else if (_units[i] is TowerUnit tower && tower._hasFirstUpgrade)
				{
					sameTower = false;
				}
				if (unitTypes.Keys.Contains(units[i]._internalName))
				{
					unitTypes[units[i]._internalName] += 1;
					_unitsContainer.GetNode(units[i]._internalName).GetNode<Label>("Count").Text = unitTypes[units[i]._internalName].ToString();
				}
				else
				{
					unitTypes.Add(units[i]._internalName, 1);
					VBoxContainer container = new();
					_unitsContainer.AddChild(container);
					container.Name = units[i]._internalName;

					Label label_ = new Label();
					label_.Text = UnitManager.InternalNameToName(_units[i]._internalName);
					container.AddChild(label_);

					Label count = new Label();
					count.Name = "Count";
					count.Text = "1";
					container.AddChild(count);

					TextureRect icon = new TextureRect();
					icon.Size = new(50, 50);
					icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
					icon.Texture = _units[i].GetIconTexture();
					container.AddChild(icon);

				}
					
			}
			_unitsContainer.Show();
			if (sameTower)
			{
				foreach (Node body in _upgradeContainer.GetChildren())
				{
					body.QueueFree();
				}
				_upgradeContainer.Show();

				TowerUnit tower = (TowerUnit)_units[0];
				VBoxContainer upgrade = new VBoxContainer();
				Label cost = new();
				cost.Text = "$" + tower._firstUpgradeCost.ToString();
				upgrade.AddChild(cost);

				foreach (EffectResource effect in tower._firstUpgrade)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, tower._firstUpgradeName, tower._firstUpgrade);
				upgradeButton.Pressed += (() =>
				{
					foreach (TowerUnit tower_ in _units)
					{
						if (_tdManager._money >= tower_._firstUpgradeCost)
						{
							_tdManager.SpendMoneyOnTower(tower_._firstUpgradeCost);
							tower_.UpgradeFirst();
						}
					}
					UpdateSelectedUnits(units);
				});
				upgrade.AddChild(upgradeButton);
				_upgradeContainer.AddChild(upgrade);
			}
		}
	}

	public void UpdateAllUnitInfo()
	{
		_infoContainer.Show();
		//_commandContainer.Show();

		Unit unit = _unit;

		unit.HideAttackRange();
		unit.DisplayAttackRange();

		_nameLabel.Text = unit._name;
		_hpLabel.Text = "Hp: " + unit._hp.ToString() + "/" + unit.GetHpMax().ToString();
		_speedLabel.Text = "Move speed: " + unit.GetSpeed().ToString();

		foreach (Node body in _effectsContainer.GetChildren())
		{
			body.QueueFree();
		}

		foreach (EffectResource effect in _unit._effects)
		{
			if (effect is StatsIncreaseResource || effect is SpawnerStatsIncreaseResource)
			{
				continue;
			}
			VBoxContainer container = new();
			HoverInfoLabel effectName = effect.MakeEffectTooltip(false);
			container.AddChild(effectName);
			_effectsContainer.AddChild(container);
		}

		//foreach (Node body in _commandContainer.GetChildren())
		//{
		//	body.QueueFree();
		//}

		//foreach (Command command in _unit._commandQueue)
		//{
		//	VBoxContainer container = new();
		//	Label commandName = new();
		//	commandName.Text = command.ToString();
		//	container.AddChild(commandName);
		//	_commandContainer.AddChild(container);
		//}

		if (_unit._weapon != null)
		{
			_weaponContainer.Show();
			_damageLabel.Text = "Weapon Damage: " + _unit._weapon.GetDamage().ToString();
			_cooldownLabel.Text = "Weapon Cooldown: " + _unit._weapon.GetCooldown().ToString();
			_dpsLabel.Text = "DPS: " + _unit._weapon.GetDPS().ToString();
			_rangeLabel.Text = "Range: " + _unit._weapon.GetRange().ToString();
		}
		else
		{
			_weaponContainer.Hide();
		}

		if (_unit is TowerUnit tower)
		{
			_totalCostLabel.Text = "Total Cost: $" + tower.GetTotalCost().ToString();

			_totalCostLabel.Show();

			_hpLabel.Hide();
			_armorLabel.Hide();
			_shieldLabel.Hide();
			_speedLabel.Hide();

			foreach (Node body in _upgradeContainer.GetChildren())
			{
				body.QueueFree();
			}


			if (!tower._hasFirstUpgrade)
			{
				if (tower._firstUpgrade is null)
				{
					return;
				}
				VBoxContainer upgrade = new VBoxContainer();
				Label cost = new();
				cost.Text = "$" + tower._firstUpgradeCost.ToString();
				upgrade.AddChild(cost);

				foreach (EffectResource effect in tower._firstUpgrade)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, tower._firstUpgradeName, tower._firstUpgrade);
				upgradeButton.Pressed += (() =>
				{
					if (_tdManager._money >= tower._firstUpgradeCost)
					{
						_tdManager.SpendMoneyOnTower(tower._firstUpgradeCost);
						tower.UpgradeFirst();
					}
				});
				upgrade.AddChild(upgradeButton);
				_upgradeContainer.AddChild(upgrade);
			}
			else if ((!tower._hasSecondUpgrade[0] || !tower._hasSecondUpgrade[1] || !tower._hasSecondUpgrade[2]) && tower._unlockedSecondUpgrades)
			{
				if (tower._secondUpgradeA is null)
				{
					return;
				}
				if (!tower._hasSecondUpgrade[0])
				{
					VBoxContainer upgradeA = new VBoxContainer();
					Label cost = new();
					cost.Text = "$" + tower._secondUpgradeACost.ToString();
					upgradeA.AddChild(cost);

					tower._secondUpgradeA.SetDescription();
					Button upgradeButton = tower._secondUpgradeA.MakeEffectTooltip(true);
					upgradeButton.Pressed += (() =>
					{
						if (_tdManager._money >= tower._secondUpgradeACost)
						{
							_tdManager.SpendMoneyOnTower(tower._secondUpgradeACost);
							tower.UpgradeSecondA();
						}

					});
					upgradeA.AddChild(upgradeButton);
					_upgradeContainer.AddChild(upgradeA);
				}
				if (!tower._hasSecondUpgrade[1])
				{
					VBoxContainer upgradeB = new VBoxContainer();
					Label cost = new();
					cost.Text = "$" + tower._secondUpgradeBCost.ToString();
					upgradeB.AddChild(cost);

					tower._secondUpgradeB.SetDescription();
					Button upgradeButton = tower._secondUpgradeB.MakeEffectTooltip(true);
					upgradeButton.Pressed += (() =>
					{
						if (_tdManager._money >= tower._secondUpgradeBCost)
						{
							_tdManager.SpendMoneyOnTower(tower._secondUpgradeBCost);
							tower.UpgradeSecondB();
						}

					});
					upgradeB.AddChild(upgradeButton);
					_upgradeContainer.AddChild(upgradeB);
				}
				if (!tower._hasSecondUpgrade[2])
				{
					VBoxContainer upgradeC = new VBoxContainer();
					Label cost = new();
					cost.Text = "$" + tower._secondUpgradeCCost.ToString();
					upgradeC.AddChild(cost);

					tower._secondUpgradeC.SetDescription();
					Button upgradeButton = tower._secondUpgradeC.MakeEffectTooltip(true);
					upgradeButton.Pressed += (() =>
					{
						if (_tdManager._money >= tower._secondUpgradeCCost)
						{
							_tdManager.SpendMoneyOnTower(tower._secondUpgradeCCost);
							tower.UpgradeSecondC();
						}

					});
					upgradeC.AddChild(upgradeButton);
					_upgradeContainer.AddChild(upgradeC);
				}
			}
			else if (!tower._hasThirdUpgrade && tower._unlockedThirdUpgrade)
			{
				if (tower._thirdUpgrade.Count == 0)
				{
					return;
				}
				VBoxContainer upgrade = new VBoxContainer();
				Label cost = new();
				cost.Text = "$" + tower._thirdUpgradeCost.ToString();
				upgrade.AddChild(cost);

				foreach (EffectResource effect in tower._thirdUpgrade)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, tower._thirdUpgradeName, tower._thirdUpgrade);
				upgradeButton.Pressed += (() =>
				{
					if (_tdManager._money >= tower._thirdUpgradeCost)
					{
						_tdManager.SpendMoneyOnTower(tower._thirdUpgradeCost);
						tower.UpgradeThird();
					}

				});
				upgrade.AddChild(upgradeButton);
				_upgradeContainer.AddChild(upgrade);
			}
			else if (!tower._hasFourthUpgrade && tower._unlockedFourthUpgrade)
			{
				if (tower._fourthUpgrade.Count == 0)
				{
					return;
				}
				VBoxContainer upgrade = new VBoxContainer();
				Label cost = new();
				cost.Text = "$" + tower._fourthUpgradeCost.ToString();
				upgrade.AddChild(cost);

				foreach (EffectResource effect in tower._fourthUpgrade)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, tower._fourthUpgradeName, tower._fourthUpgrade);
				upgradeButton.Pressed += (() =>
				{
					if (_tdManager._money >= tower._fourthUpgradeCost)
					{
						_tdManager.SpendMoneyOnTower(tower._fourthUpgradeCost);
						tower.UpgradeFourth();
					}

				});
				upgrade.AddChild(upgradeButton);
				_upgradeContainer.AddChild(upgrade);
			}


			_upgradeContainer.Show();

			if (tower.GetIncome() > 0)
			{
				_incomeLabel.Text = "Maximum Income: $" + tower.GetIncome();
				_incomeLabel.Show();
			}
			else
			{
				if (_incomeLabel.Visible)
				{
					_incomeLabel.Hide();
				}
			}

			

			if (tower._weapon is not null)
			{
				_priorityContainer.Show();

				Node oldUp = _priorityContainer.GetNodeOrNull("Up");
				if (oldUp != null)
				{
					_priorityContainer.RemoveChild(oldUp);
				}

				Node oldDown = _priorityContainer.GetNodeOrNull("Down");
				if (oldDown != null)
				{
					_priorityContainer.RemoveChild(oldDown);
				}

				Button upPriorityButton = new Button();
				upPriorityButton.Name = "Up";
				upPriorityButton.Text = "↑";
				upPriorityButton.Pressed += () => tower.LastTargetPriority();
				_priorityContainer.AddChild(upPriorityButton);
				_priorityContainer.MoveChild(upPriorityButton, 0);

				Button downPriorityButton = new Button();
				downPriorityButton.Name = "Down";
				downPriorityButton.Text = "↓";
				downPriorityButton.Pressed += () => tower.NextTargetPriority();
				_priorityContainer.AddChild(downPriorityButton);

				switch (tower._targetPriority)
				{
					case TowerUnit.TargetPriority.First:
						_priorityLabel.Text = "First";
						break;
					case TowerUnit.TargetPriority.Last:
						_priorityLabel.Text = "Last";
						break;
					case TowerUnit.TargetPriority.Closest:
						_priorityLabel.Text = "Closest";
						break;
					case TowerUnit.TargetPriority.Strongest:
						_priorityLabel.Text = "Strongest";
						break;
					default:
						_priorityLabel.Text = ((int)tower._targetPriority).ToString();
						break;
				}
			}
			else
			{
				if (_priorityContainer.Visible)
				{
					_priorityContainer.Hide();
				}
			}
		}
		else
		{
			_totalCostLabel.Hide();
			_incomeLabel.Hide();
			unit.HideAttackRange();

			_hpLabel.Show();
			_armorLabel.Show();
			_speedLabel.Show();

			_hpLabel.Text = "Hp: " + unit._hp.ToString() + "/" + unit.GetHpMax().ToString();
			if (unit._shield > 0)
			{
				_shieldLabel.Text = "Shield: " + unit._shield.ToString() + "/" + unit.GetHpMax().ToString();
				_shieldLabel.Show();
			}
			else
			{
				if (_shieldLabel.Visible)
				{
					_shieldLabel.Hide();
				}
			}

			if (unit.GetSpeed() > 0)
			{
				_speedLabel.Text = "Move speed: " + unit.GetSpeed().ToString();
			}
			else
			{
				_speedLabel.Text = "Stationary";
			}

			_armorLabel.Text = "Armor: " + unit.GetArmor().ToString();
		}

		if (_unit is InvaderUnit invader)
		{
			_moneyDroppedLabel.Show();
			_moneyDroppedLabel.Text = "Drops $" + invader.GetMoneyDropped().ToString();
		}
		else
		{
			_moneyDroppedLabel.Hide();
		}

		if (_unit is Spawner spawner && spawner._data._units.Count > 0)
		{
			_spawnLabel.Text = "Spawns " + spawner.GetSpawns();
			_spawnLabel.Show();
		}
		else
		{
			_spawnLabel.Hide();
		}

			//for (int i = _commandContainer.GetChildCount() - 1; i >= 0; i--)
			//{
			//	_commandContainer.GetChild(i).QueueFree();
			//}
			//Command command = unit._currentCommand;
			//Label label = new Label();
			//label.Text = command.GetDescription();
			//_commandContainer.AddChild(label);

			//for (int i = 0; i < unit._commandQueue.Count; i++)
			//{
			//	Label label_ = new Label();
			//	label_.Text = unit._commandQueue[i].GetDescription();
			//	_commandContainer.AddChild(label_);
			//}
		_tdManager._towerManager.UpdateIncomeDisplay();
	}

	// updates texts without refreshing upgrade and effect buttons
	public void UpdateUnitStatsInfo()
	{

		Unit unit = _unit;

		unit.HideAttackRange();
		unit.DisplayAttackRange();

		_nameLabel.Text = unit._name;
		_hpLabel.Text = "Hp: " + unit._hp.ToString() + "/" + unit.GetHpMax().ToString();
		_speedLabel.Text = "Move speed: " + unit.GetSpeed().ToString();

		if (_unit._weapon != null)
		{
			_damageLabel.Text = "Weapon Damage: " + _unit._weapon.GetDamage().ToString();
			_cooldownLabel.Text = "Weapon Cooldown: " + _unit._weapon.GetCooldown().ToString();
			_dpsLabel.Text = "DPS: " + _unit._weapon.GetDPS().ToString();
			_rangeLabel.Text = "Range: " + _unit._weapon.GetRange().ToString();
		}
		else
		{
		}

		if (_unit is TowerUnit tower)
		{
			_totalCostLabel.Text = "Total Cost: $" + tower.GetTotalCost().ToString();

			if (tower.GetIncome() > 0)
			{
				_incomeLabel.Text = "Maximum Income: $" + tower.GetIncome();
			}
		}
		else
		{
			_hpLabel.Text = "Hp: " + unit._hp.ToString() + "/" + unit.GetHpMax().ToString();
			if (unit._shield > 0)
			{
				_shieldLabel.Text = "Shield: " + unit._shield.ToString() + "/" + unit.GetHpMax().ToString();
				_shieldLabel.Show();
			}
			else
			{
				if (_shieldLabel.Visible)
				{
					_shieldLabel.Hide();
				}
			}

			if (unit.GetSpeed() > 0)
			{
				_speedLabel.Text = "Move speed: " + unit.GetSpeed().ToString();
			}
			else
			{
				_speedLabel.Text = "Stationary";
			}

			_armorLabel.Text = "Armor: " + unit.GetArmor().ToString();
		}

		if (_unit is InvaderUnit invader)
		{
			_moneyDroppedLabel.Text = "Drops $" + invader.GetMoneyDropped().ToString();
		}

		if (_unit is Spawner spawner && spawner._data._units.Count > 0)
		{
			_spawnLabel.Text = "Spawns " + spawner.GetSpawns();
		}
	}

	public void HideUnitInfo()
	{
		_unit?.Disconnect(Unit.SignalName.UpdateInfo, Callable.From(UpdateAllUnitInfo));
		_unit?.Disconnect(Unit.SignalName.UpdateStatsInfo, Callable.From(UpdateUnitStatsInfo));
		_unit = null;
		_infoContainer.Hide();
		_upgradeContainer.Hide();
		//_commandContainer.Hide();
		_unitsContainer.Hide();
	}
}