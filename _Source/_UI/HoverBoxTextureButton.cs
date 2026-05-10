using Godot;
using System;

public partial class HoverBoxTextureButton : TextureButton
{
	[Export] public Control _popUpBox; // Assign your PanelContainer here in the Inspector
	[Export] public Vector2 _offSet = new Vector2(-60, 40); // Offset from this button

	public override void _Ready()
	{
		// Connect the signals
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		AddChild(_popUpBox);
		_popUpBox.Hide();
	}

	//public void Initialize()
	//{
	//	MouseEntered += OnMouseEntered;
	//	MouseExited += OnMouseExited;
	//	AddChild(_popUpBox);
	//	_popUpBox.Hide();
	//}

	public override void _Process(double delta)
	{
		if (_popUpBox != null && _popUpBox.Visible)
		{
			_popUpBox.GlobalPosition = GlobalPosition + _offSet;
		}
	}

	private void OnMouseEntered()
	{
		if (_popUpBox != null) _popUpBox.Visible = true;
	}

	private void OnMouseExited()
	{
		if (_popUpBox != null) _popUpBox.Visible = false;
	}
}