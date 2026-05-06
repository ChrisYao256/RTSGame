using Godot;
using RTSGame.Units;
using System;
using System.Collections.Generic;

public partial class UnitInfoPanel : CanvasLayer
{
	private Label _nameLabel;
	private Label _hpLabel;
	private Label _speedLabel;
	private Label _commandLabel;
	private PanelContainer _container;

	private List<Unit> _units;

	public override void _Ready()
	{
		// Link our C# variables to the nodes in the scene
		_nameLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/UnitNameLabel");
		_hpLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/Hp");
		_speedLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/SpeedLabel");
		_commandLabel = GetNode<Label>("PanelContainer/HBoxContainer/Command");
		_container = GetNode<PanelContainer>("PanelContainer");
	}

	// This method will be called by your Selection Manager
	public void UpdateSelectedUnits(List<Unit> units)
	{
		_units = units;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_units is null)
		{
			return;
		}
		if (_units.Count == 1)
		{
			Unit unit = _units[0];
			_nameLabel.Text = unit._name;
			_hpLabel.Text = unit._hp.ToString() + "/" + unit._hpMax.ToString();
			_speedLabel.Text = unit._moveSpeed.ToString();
			_commandLabel.Text = unit._currentCommand.ToString();
		}

	}
}