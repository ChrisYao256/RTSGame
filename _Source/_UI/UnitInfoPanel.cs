using Godot;
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
	private VBoxContainer _infoContainer;
	private VBoxContainer _commandContainer;
	private BoxContainer _unitsContainer;
	private HBoxContainer _hBoxContainer;
	private PanelContainer _container;

	private List<Unit> _units;

	public override void _Ready()
	{
		// Link our C# variables to the nodes in the scene
		_nameLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/UnitNameLabel");
		_hpLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/Hp");
		_speedLabel = GetNode<Label>("PanelContainer/HBoxContainer/UnitInfo/SpeedLabel");
		_positionLabel = GetNode<Label>("PanelContainer/HBoxContainer/Position");
		_infoContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/UnitInfo");

		_commandContainer = GetNode<VBoxContainer>("PanelContainer/HBoxContainer/CommandContainer");

		_unitsContainer = GetNode<BoxContainer>("PanelContainer/HBoxContainer/UnitsContainer");

		_hBoxContainer = GetNode<HBoxContainer>("PanelContainer/HBoxContainer");
		_container = GetNode<PanelContainer>("PanelContainer");
	}

	// This method will be called by your Selection Manager
	public void UpdateSelectedUnits(List<Unit> units)
	{
		_units = units;
		if (_units is null)
		{
			return;
		}
		if (_units.Count == 1)
		{
			for (int i = _unitsContainer.GetChildCount() - 1; i >= 0; i--)
			{
				_unitsContainer.GetChild(i).QueueFree();
			}

			_infoContainer.Show();
			_commandContainer.Show();
			_positionLabel.Show();

			Unit unit = _units[0];
			_nameLabel.Text = unit._name;
			_hpLabel.Text = unit._hp.ToString() + "/" + unit._hpMax.ToString();
			_speedLabel.Text = unit._moveSpeed.ToString();

			for (int i = _commandContainer.GetChildCount() - 1; i >= 0; i--)
			{
				_commandContainer.GetChild(i).QueueFree();
			}
			Command command = unit._currentCommand;
			Label label = new Label();
			label.Text = command.GetDescription();
			_commandContainer.AddChild(label);

			for (int i = 0; i < unit._commandQueue.Count; i++)
			{
				Label label_ = new Label();
				label_.Text = unit._commandQueue[i].GetDescription();
				_commandContainer.AddChild(label_);
			}
			_positionLabel.Text = unit.GlobalPosition.ToString();
		}
		if (_units.Count > 1)
		{
			_infoContainer.Hide();
			_commandContainer.Hide();
			_positionLabel.Hide();

			for (int i = 0; i < _units.Count; i++)
			{
				Label label_ = new Label();
				label_.Text = _units[i]._name;
				_unitsContainer.AddChild(label_);
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		
		//for (int i = _hBoxContainer.GetChildCount() - 1; i >= 0; i--)
		//{
		//	_hBoxContainer.GetChild(i).QueueFree();
		//}
		
	}
}