using Godot;
using RTSGame.Source;
using RTSGame.Units;
using System;
using System.Collections.Generic;

public partial class UnitInfoPanel : CanvasLayer
{
	private HBoxContainer _infoContainer;

	private List<Unit> _units;
	private Unit _unit;

	private TDManager _tdManager;

	public override void _Ready()
	{
		// Link our C# variables to the nodes in the scene
		//_nameLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/BasicInfoContainer/VBoxContainer/UnitNameLabel");
		//_hpLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/BasicInfoContainer/VBoxContainer/Hp");
		//_shieldLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/BasicInfoContainer/VBoxContainer/Shield");
		//_speedLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/BasicInfoContainer/VBoxContainer/SpeedLabel");
		//_armorLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/BasicInfoContainer/VBoxContainer/ArmorLabel");
		//_totalCostLabel = GetNode<RichTextLabel>("PanelContainer/HBoxContainer/UnitInfo/IncomeContainer/VBoxContainer/TotalCostLabel");
		//_incomeLabel = GetNode<RichTextLabel>("PanelContainer/HBoxContainer/UnitInfo/IncomeContainer/VBoxContainer/IncomeLabel");
		//_moneyDroppedLabel = GetNode<RichTextLabel>("PanelContainer/HBoxContainer/UnitInfo/IncomeContainer/VBoxContainer/MoneyDroppedLabel");
		//_spawnLabel = GetNode<RichTextLabel>("PanelContainer/HBoxContainer/UnitInfo/IncomeContainer/VBoxContainer/SpawnLabel");
		//_weaponContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/UnitInfo/WeaponContainer/VBoxContainer");
		//_damageLabel = _weaponContainer.GetNode<Label>("DamageLabel");
		//_cooldownLabel = _weaponContainer.GetNode<Label>("CooldownLabel");
		//_rangeLabel = _weaponContainer.GetNode<Label>("DPSLabel");
		//_dpsLabel = _weaponContainer.GetNode<Label>("RangeLabel");
		//_spawnContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/UnitInfo/SpawnContainer/VBoxContainer");

		//_priorityContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/UnitInfo/TargetPriorityContainer");
		//_priorityLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/TargetPriorityContainer/TargetPriority");
		//_largeEffectsContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer/UnitInfo/Effects");
		//_smallEffectsContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/UnitInfo/SmallEffects");

		//_upgradeContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer/Upgrades/HBoxContainer");


		//_unitsContainer = GetNode<BoxContainer>("PanelContainer/HBoxContainer/UnitsContainer");

		//_hBoxContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer");
		//_container = GetNode<PanelContainer>("PanelContainer");

		_infoContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer/UnitInfo");

		_tdManager = GetParent().GetNode<TDManager>("TdManager");
	}

	// This method will be called by your Selection Manager
	public void UpdateSelectedUnits(List<Unit> units)
	{
		ResetUnitInfo();
		_units = units;
		if (_units is null)
		{
			return;
		}
		if (_units.Count == 1)
		{
			ResetUnitInfo();
			_unit = units[0];
			_unit.Connect(Unit.SignalName.UpdateInfo, Callable.From(UpdateAllUnitInfo));
			_unit.Connect(Unit.SignalName.UpdateStatsInfo, Callable.From(UpdateUnitStatsInfo));
			MakeNewUnitInfo();
		}
		//if (_units.Count > 1)
		//{
		//	for (int i = _unitsContainer.GetChildCount() - 1; i >= 0; i--)
		//	{
		//		_unitsContainer.GetChild(i).Free();
		//	}
		//	Dictionary<string, int> unitTypes = new Dictionary<string, int>();
		//	for (int i = 0; i < _units.Count; i++)
		//	{
		//		if (unitTypes.Keys.Contains(units[i]._internalName))
		//		{
		//			unitTypes[units[i]._internalName] += 1;
		//			_unitsContainer.GetNode(units[i]._internalName).GetNode<Label>("Count").Text = unitTypes[units[i]._internalName].ToString();
		//		}
		//		else
		//		{
		//			unitTypes.Add(units[i]._internalName, 1);
		//			VBoxContainer container = new();
		//			_unitsContainer.AddChild(container);
		//			container.Name = units[i]._internalName;

		//			Label label_ = new Label();
		//			label_.Text = UnitManager.InternalNameToName(_units[i]._internalName);
		//			container.AddChild(label_);

		//			Label count = new Label();
		//			count.Name = "Count";
		//			count.Text = "1";
		//			container.AddChild(count);

		//			TextureRect icon = new TextureRect();
		//			icon.Size = new(50, 50);
		//			icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
		//			icon.Texture = _units[i].GetIconTexture();
		//			container.AddChild(icon);

		//		}
					
		//	}
		//	_unitsContainer.Show();
		//}
	}

	public Unit GetSelectedUnit()
	{
		if (_units is not null && _units.Count == 1)
		{
			return _units[0];
		}
		else
		{
			return null;
		}
		
	}

	public void MakeNewUnitInfo()
	{
		//_infoContainer.Show();
		Unit unit = _unit;
		Godot.Collections.Dictionary<string, PanelContainer> infoContainers = _unit.MakeUnitInfoContainer();

		foreach (var infoContainer in infoContainers.Values)
		{
			_infoContainer.AddChild(infoContainer);
		}
	}

	public void UpdateAllUnitInfo() // does not change selected unit
	{
		
		//_commandContainer.Show();

		_unit.UpdateUnitInfoContainer(true);

		/*

		unit.HideAttackRange();
		unit.DisplayAttackRange();

		_nameLabel.Text = unit._name;
		_hpLabel.Text = "Hp: " + unit._hp.ToString() + "/" + unit.GetHpMax().ToString();
		_speedLabel.Text = "Move speed: " + unit.GetSpeed().ToString();

		foreach (Node body in _largeEffectsContainer.GetChildren())
		{
			body.QueueFree();
		}
		foreach (Node body in _smallEffectsContainer.GetChildren())
		{
			body.QueueFree();
		}

		foreach (EffectResource effect in _unit._effects)
		{
			switch (effect._displayType)
			{
				case (EffectResource.DisplayTypes.Large):
					VBoxContainer container = new();
					PanelContainer effectName = effect.MakeFullEffectDescription();
					container.AddChild(effectName);
					_largeEffectsContainer.AddChild(container);
					break;
				case (EffectResource.DisplayTypes.Small):
					VBoxContainer container1 = new();
					HoverInfoLabel effectName1 = effect.MakeEffectTooltip(false);
					container1.AddChild(effectName1);
					_smallEffectsContainer.AddChild(container1);
					break;
				case (EffectResource.DisplayTypes.Hidden):
					continue;
			}
		}

		if (_unit._weapon != null)
		{
			_weaponContainer.Show();
			_damageLabel.Text = "Weapon Damage: " + _unit._weapon.GetDamage().ToString();
			_cooldownLabel.Text = "Weapon Cooldown: " + _unit._weapon.GetCooldown().ToString("F2");
			_dpsLabel.Text = "DPS: " + _unit._weapon.GetDPS().ToString("F0");
			_rangeLabel.Text = "Range: " + _unit._weapon.GetRange().ToString();
		}
		else
		{
			_weaponContainer.Hide();
		}

		if (_unit is TowerUnit tower)
		{
			_totalCostLabel.Text = "Total Cost: " + Utils.MakeMoneyText(tower.GetTotalCost());

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
				RichTextLabel cost = new();
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(tower._firstUpgradeCost);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in tower._firstUpgrade)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, tower._firstUpgradeName, tower._firstUpgrade);
				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(tower._firstUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(tower._firstUpgradeCost);
						tower.UpgradeFirst();
					}
				});
				upgrade.AddChild(upgradeButton);
				_upgradeContainer.AddChild(upgrade);
			}
			else if ((!tower._hasSecondUpgrade) && tower._unlockedSecondUpgrade)
			{
				if (tower._secondUpgrade is null)
				{
					return;
				}
				VBoxContainer upgrade = new VBoxContainer();
				RichTextLabel cost = new();
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(tower._secondUpgradeCost);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in tower._secondUpgrade)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, tower._secondUpgradeName, tower._secondUpgrade);
				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(tower._secondUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(tower._secondUpgradeCost);
						tower.UpgradeSecond();
					}

				});
				upgrade.AddChild(upgradeButton);
				_upgradeContainer.AddChild(upgrade);
			}
			else if (!tower._hasThirdUpgrade && tower._unlockedThirdUpgrade)
			{
				if (tower._thirdUpgrade.Count == 0)
				{
					return;
				}
				VBoxContainer upgrade = new VBoxContainer();
				RichTextLabel cost = new();
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(tower._thirdUpgradeCost);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in tower._thirdUpgrade)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, tower._thirdUpgradeName, tower._thirdUpgrade);
				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(tower._thirdUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(tower._thirdUpgradeCost);
						tower.UpgradeThird();
					}

				});
				upgrade.AddChild(upgradeButton);
				_upgradeContainer.AddChild(upgrade);
			}
			else if (!tower._hasFourthUpgrade && tower._unlockedFourthUpgrade)
			{
				if (tower._fourthUpgradeA is not null && tower._fourthUpgradeA.Count != 0)
				{
					VBoxContainer upgrade = new VBoxContainer();
					RichTextLabel cost = new();
					cost.FitContent = true;
					cost.BbcodeEnabled = true;
					cost.Text = Utils.MakeMoneyText(tower._fourthUpgradeACost);
					upgrade.AddChild(cost);

					foreach (EffectResource effect in tower._fourthUpgradeA)
					{
						effect.SetDescription();
					}

					Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, tower._fourthUpgradeAName, tower._fourthUpgradeA);
					upgradeButton.Pressed += (() =>
					{
						if (Utils.VectorLeq(tower._fourthUpgradeACost, _tdManager._money))
						{
							_tdManager.SpendMoney(tower._fourthUpgradeACost);
							tower.UpgradeFourthA();
						}

					});
					upgrade.AddChild(upgradeButton);

					TextureRect image = new();
					image.Texture = tower._fourthUpgradeATexture;
					image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
					upgrade.AddChild(image);

					_upgradeContainer.AddChild(upgrade);
				}
				if (tower._fourthUpgradeB is not null && tower._fourthUpgradeB.Count!= 0)
				{
					VBoxContainer upgrade = new VBoxContainer();
					RichTextLabel cost = new();
					cost.FitContent = true;
					cost.BbcodeEnabled = true;
					cost.Text = Utils.MakeMoneyText(tower._fourthUpgradeBCost);
					upgrade.AddChild(cost);

					foreach (EffectResource effect in tower._fourthUpgradeB)
					{
						effect.SetDescription();
					}

					Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, tower._fourthUpgradeBName, tower._fourthUpgradeB);
					upgradeButton.Pressed += (() =>
					{
						if (Utils.VectorLeq(tower._fourthUpgradeBCost, _tdManager._money))
						{
							_tdManager.SpendMoney(tower._fourthUpgradeBCost);
							tower.UpgradeFourthB();
						}

					});
					upgrade.AddChild(upgradeButton);

					TextureRect image = new();
					image.Texture = tower._fourthUpgradeBTexture;
					image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
					upgrade.AddChild(image);

					_upgradeContainer.AddChild(upgrade);
				}
			}


			_upgradeContainer.Show();

			if (tower.GetIncome() != new Vector4I(0,0,0,0))
			{
				_incomeLabel.Text = "Maximum Income: " + Utils.MakeMoneyText(tower.GetIncome());
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
			_moneyDroppedLabel.Text = "Drops " + Utils.MakeMoneyText(invader.GetMoneyDropped());
		}
		else
		{
			_moneyDroppedLabel.Hide();
		}

		if (_unit is Spawner spawner && spawner._data._units.Count > 0)
		{
			_spawnLabel.Text = "Spawns " + spawner.GetSpawns();
			_spawnLabel.Show();

			Unit spawnedUnit = UnitManager.GetUnit(spawner._data._units[0]);
			
			foreach (var child in _spawnContainer.GetChildren())
			{
				child.QueueFree();
			}

			spawnedUnit._hpMaxModifier += spawner._data._hpBuff;
			spawnedUnit.IncreaseSpeedModifier(spawner._data._speedBuff);
			((InvaderUnit)spawnedUnit).IncreaseMoneyModifier(spawner._data._moneyBuff);

			Label name = new();
			name.Text = spawnedUnit._name;
			_spawnContainer.AddChild(name);

			Label hpLabel = new();
			hpLabel.Text = "Max Hp: " + spawnedUnit.GetHpMax().ToString();
			_spawnContainer.AddChild(hpLabel);

			Label speedLabel = new();
			speedLabel.Text = "Move speed: " + spawnedUnit.GetSpeed().ToString();
			_spawnContainer.AddChild(speedLabel);

			RichTextLabel moneyDrop = new();
			moneyDrop.Text = "Drops " + Utils.MakeMoneyText(((InvaderUnit)spawnedUnit).GetMoneyDropped());
			moneyDrop.BbcodeEnabled = true;
			moneyDrop.FitContent = true;
			_spawnContainer.AddChild(moneyDrop);

			_spawnContainer.Show();
		}
		else
		{
			_spawnLabel.Hide();
			_spawnContainer.Hide();
		}
		_tdManager._towerManager.UpdateIncomeDisplay();
		*/
	}

	// updates texts without refreshing upgrade and effect buttons
	public void UpdateUnitStatsInfo() // doesn't update effects
	{
		_unit.UpdateUnitInfoContainer(false);
		/*
		Unit unit = _unit;

		unit.HideAttackRange();
		unit.DisplayAttackRange();

		_nameLabel.Text = unit._name;
		_hpLabel.Text = "Hp: " + unit._hp.ToString() + "/" + unit.GetHpMax().ToString();
		_speedLabel.Text = "Move speed: " + unit.GetSpeed().ToString();

		if (_unit._weapon != null)
		{
			_damageLabel.Text = "Weapon Damage: " + _unit._weapon.GetDamage().ToString();
			_cooldownLabel.Text = "Weapon Cooldown: " + _unit._weapon.GetCooldown().ToString("F2");
			_dpsLabel.Text = "DPS: " + _unit._weapon.GetDPS().ToString("F0");
			_rangeLabel.Text = "Range: " + _unit._weapon.GetRange().ToString();
		}
		else
		{
		}

		if (_unit is TowerUnit tower)
		{
			_totalCostLabel.Text = "Total Cost: " + Utils.MakeMoneyText(tower.GetTotalCost());

			if (tower.GetIncome() != new Vector4I(0,0,0,0))
			{
				_incomeLabel.Text = "Maximum Income: " + Utils.MakeMoneyText(tower.GetIncome());
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
			_moneyDroppedLabel.Text = "Drops " + Utils.MakeMoneyText(invader.GetMoneyDropped());
		}

		if (_unit is Spawner spawner && spawner._data._units.Count > 0)
		{
			_spawnLabel.Text = "Spawns " + spawner.GetSpawns();
		}
		*/
	}

	public void ResetUnitInfo()
	{
		_unit?.Disconnect(Unit.SignalName.UpdateInfo, Callable.From(UpdateAllUnitInfo));
		_unit?.Disconnect(Unit.SignalName.UpdateStatsInfo, Callable.From(UpdateUnitStatsInfo));
		_unit?.ResetUnitInfoContainer();
		_unit = null;
		//_infoContainer.Hide();
		//_upgradeContainer.Hide();
		//_unitsContainer.Hide();
	}
}