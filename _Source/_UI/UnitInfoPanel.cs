using Godot;
using RTSGame.Source;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class UnitInfoPanel : CanvasLayer
{
	private Label _nameLabel;
	private Label _hpLabel;
	private Label _speedLabel;
	private Label _positionLabel;
	private Label _damageLabel;
	private Label _cooldownLabel;
	private Label _dpsLabel;
	private VBoxContainer _effectsContainer;
	private HBoxContainer _infoContainer;
	private VBoxContainer _commandContainer;
	private BoxContainer _unitsContainer;
	private HBoxContainer _hBoxContainer;
	private PanelContainer _container;

	private List<Unit> _units;
	private Unit _unit;

	public override void _Ready()
	{
		// Link our C# variables to the nodes in the scene
		_nameLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/UnitNameLabel");
		_hpLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer/Hp");
		_speedLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/SpeedLabel");
		_damageLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/DamageLabel");
		_cooldownLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/CooldownLabel");
		_dpsLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/VBoxContainer2/DPSLabel");
		_effectsContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/UnitInfo/Effects");
		_positionLabel = GetNode<Label>("PanelContainer/HBoxContainer/Position");
		_infoContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer/UnitInfo");

		_commandContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/CommandContainer");

		_unitsContainer = GetNode<BoxContainer>("PanelContainer/HBoxContainer/UnitsContainer");

		_hBoxContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer");
		_container = GetNode<PanelContainer>("PanelContainer");
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
		_nameLabel.Text = unit._name;
		_hpLabel.Text = "Hp: " + unit._hp.ToString() + "/" + unit._hpMax.ToString();
		_speedLabel.Text = "Move speed: " + unit._moveSpeed.ToString();

		foreach (Node body in _effectsContainer.GetChildren())
		{
			body.QueueFree();
		}

		foreach (Effect effect in _unit._effects)
		{
			VBoxContainer container = new();
			Label label_ = new Label();
			label_.Text = effect._effectName;
			container.AddChild(label_);
			_effectsContainer.AddChild(container);
		}

		if (_unit._weapon != null)
		{
			_damageLabel.Text = "Weapon Damage: " + _unit._weapon._damage.ToString() + " + " + _unit._weapon._damageModifier.ToString();
			_cooldownLabel.Text = "Weapon Cooldown: " + _unit._weapon._attackCooldown.ToString();
			_dpsLabel.Text = "DPS: " + _unit._weapon.GetDPS().ToString();
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
		//_commandContainer.Hide();
		_unitsContainer.Hide();
	}
}