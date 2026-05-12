using Godot;
using RTSGame.Source;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

public partial class UnitInfoPanel : CanvasLayer
{
	private Label _nameLabel;
	private Label _hpLabel;
	private Label _speedLabel;
	private Label _positionLabel;
	private VBoxContainer _weaponContainer;
	private Label _damageLabel;
	private Label _cooldownLabel;
	private Label _dpsLabel;
	private Label _rangeLabel;
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
		_speedLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/SpeedLabel");
		_weaponContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2");
		_damageLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/DamageLabel");
		_cooldownLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/CooldownLabel");
		_rangeLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/RangeLabel");
		_dpsLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/DPSLabel");
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
			_unit.Connect(Unit.SignalName.UpdateInfo, Callable.From(UpdateUnitInfo));
			UpdateUnitInfo();
		}
		if (_units.Count > 1)
		{
			for (int i = _unitsContainer.GetChildCount() - 1; i >= 0; i--)
			{
				_unitsContainer.GetChild(i).Free();
			}
			Dictionary<string, int> unitTypes = new Dictionary<string, int>();
			for (int i = 0; i < _units.Count; i++)
			{
				if (unitTypes.Keys.Contains(units[i]._name))
				{
					unitTypes[units[i]._name] += 1;
					_unitsContainer.GetNode(units[i]._name).GetNode<Label>("Count").Text = unitTypes[units[i]._name].ToString();
				}
				else
				{
					unitTypes.Add(units[i]._name, 1);
					VBoxContainer container = new();
					_unitsContainer.AddChild(container);
					container.Name = units[i]._name;

					Label label_ = new Label();
					label_.Text = _units[i]._name;
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
		}
	}

	public void UpdateUnitInfo()
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
			VBoxContainer container = new();
			HoverInfoLabel effectName = effect.MakeEffectTooltip(false);
			container.AddChild(effectName);
			_effectsContainer.AddChild(container);
		}

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
			foreach (Node body in _upgradeContainer.GetChildren())
			{
				body.QueueFree();
			}


			if (!tower._hasFirstUpgrade)
			{
				VBoxContainer upgrade = new VBoxContainer();
				Label cost = new();
				cost.Text = "$" + tower._firstUpgradeCost.ToString();
				upgrade.AddChild(cost);

				tower._firstUpgrade.SetDescription();
				Button upgradeButton = tower._firstUpgrade.MakeEffectTooltip(true);
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
			else
			{
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


			_upgradeContainer.Show();
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
	}

	public void HideUnitInfo()
	{
		_unit?.Disconnect(Unit.SignalName.UpdateInfo, Callable.From(UpdateUnitInfo));
		_unit = null;
		_infoContainer.Hide();
		_upgradeContainer.Hide();
		//_commandContainer.Hide();
		_unitsContainer.Hide();
	}
}