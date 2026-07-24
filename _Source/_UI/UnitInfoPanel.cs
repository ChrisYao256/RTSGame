using Godot;
using RTSGame.Source;
using RTSGame.Units;
using System;
using System.Collections.Generic;

public partial class UnitInfoPanel : CanvasLayer
{
	private HBoxContainer _infoContainer;
	private GridContainer _towersContainer;
	private VBoxContainer _buildContainer;

	private List<Unit> _units;
	private Unit _unit;

	private TowerUnit.TowerType _tab = TowerUnit.TowerType.Null;

	private TDManager _tdManager;

	private Dictionary<string, TowerUnit> _unitInstanceDictionary = new();

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
		_towersContainer = GetNode<GridContainer>("PanelContainer/Towers/GridContainer");
		_buildContainer = GetNode<VBoxContainer>("PanelContainer/Towers");

		_tdManager = GetParent().GetNode<TDManager>("TdManager");
	}

	// This method will be called by your Selection Manager
	public void UpdateSelectedUnits(List<Unit> units)
	{
		ResetUnitInfo();
		_units = units;
		if (_units is null || _units.Count == 0)
		{
			MakeAllTowersMenu(_tab);
		}
		if (_units.Count == 1)
		{
			ResetUnitInfo();
			_unit = units[0];
			_unit.Connect(Unit.SignalName.UpdateInfo, Callable.From(UpdateAllUnitInfo));
			_unit.Connect(Unit.SignalName.UpdateStatsInfo, Callable.From(UpdateUnitStatsInfo));
			_unit.Connect(Unit.SignalName.UpdateDamageDealtInfo, Callable.From(UpdateDamageDealtInfo));
			if (_unit is TowerUnit)
			{
				_unit.Connect(Unit.SignalName.UpdateUpgradeButton, Callable.From(UpdateUpgradeButton));
			}
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
		_buildContainer.Hide();
		_infoContainer.Show();
		Unit unit = _unit;
		Godot.Collections.Dictionary<string, PanelContainer> infoContainers = _unit.MakeUnitInfoContainer();

		foreach (var infoContainer in infoContainers.Values)
		{
			_infoContainer.AddChild(infoContainer);
		}
	}

	public void UpdateAllUnitInfo() // does not change selected unit
	{
		_unit.UpdateUnitInfoContainer(true);
	}

	// updates texts without refreshing upgrade and effect buttons
	public void UpdateUnitStatsInfo() // doesn't update effects
	{
		_unit.UpdateUnitInfoContainer(false);
	}

	public void UpdateDamageDealtInfo()
	{
		_unit.UpdateUnitDamageDealtInfo();
	}

	public void UpdateUpgradeButton()
	{
		((TowerUnit)_unit).UpdateUpgradeButtonProgress();
	}

	public void ResetUnitInfo()
	{
		_unit?.Disconnect(Unit.SignalName.UpdateInfo, Callable.From(UpdateAllUnitInfo));
		_unit?.Disconnect(Unit.SignalName.UpdateStatsInfo, Callable.From(UpdateUnitStatsInfo));
		_unit?.Disconnect(Unit.SignalName.UpdateDamageDealtInfo, Callable.From(UpdateDamageDealtInfo));
		if (_unit is not null && _unit is TowerUnit)
		{
			_unit?.Disconnect(Unit.SignalName.UpdateUpgradeButton, Callable.From(UpdateUpgradeButton));
		}
		_unit?.ResetUnitInfoContainer();
		_unit = null;
		//_infoContainer.Hide();
		//_upgradeContainer.Hide();
		//_unitsContainer.Hide();
		//MakeAllTowersMenu(_tab);
	}

	public void MakeAllTowersMenu(TowerUnit.TowerType tab)
	{
		if (_tab == tab && _buildContainer.Visible)
		{
			return;
		}
		_tab = tab;
		_infoContainer.Hide();
		_buildContainer.Show();
		Godot.Collections.Array<string> towers = _tdManager._availTowerList;
		foreach (Node child in _towersContainer.GetChildren())
		{
			child.QueueFree();
		}
		foreach (string name in towers)
		{
			TowerUnit unit;
			if (!_unitInstanceDictionary.Keys.Contains(name))
			{
				unit = (TowerUnit)UnitManager.GetUnit(name, true);
				_unitInstanceDictionary.Add(name, unit);
			}
			else
			{
				unit = _unitInstanceDictionary[name];
			}

			string name_ = name;
			

			if (unit._towerType != tab)
			{
				continue;
			}

			VBoxContainer container = new VBoxContainer();

			Label nameLabel = new Label();
			nameLabel.Text = unit._name;
			nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
			container.AddChild(nameLabel);

			unit._tdManager = _tdManager;

			HoverInfoImage towerButton
				= unit.MakeTowerTooltip(true);
			towerButton.Pressed += (() =>
			{
				if (Utils.VectorLeq(unit._cost, _tdManager._money))
				{
					_tdManager._towerManager.EnterPlacementMode(name_);
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
			costLabel.CustomMinimumSize = new(80, 0);
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

			_towersContainer.AddChild(container);

		}
	}
}