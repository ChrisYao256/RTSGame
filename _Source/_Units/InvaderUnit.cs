using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class InvaderUnit : Unit
{
	[Export]
	private int _moneyDropped;
	[Export] 
	private PackedScene _floatingTextScene;
	[Export]
	public int _hpDeducted = 1;
	[Export]
	public int _moneyDeducted = 1;

	public int _moneyModifier;

	public override void _Ready()
	{
		base._Ready();
		_aiControlled = true;
	}

	public void SetMoneyModifier(int money)
	{
		_moneyModifier = money;
	}

	public void IncreaseMoneyModifier(int change)
	{
		_moneyModifier += change;
	}

	protected override void Die()
	{
		if (_floatingTextScene == null) return;

		var textNode = _floatingTextScene.Instantiate<FloatingText>();

		// Set the text
		textNode.Text = $"+${GetMoneyDropped()}";
		textNode.AddThemeColorOverride("font_color", Colors.Gold);

		// Set the position to the unit's current global position
		textNode.GlobalPosition = GlobalPosition;

		// VERY IMPORTANT: Add it to the world, not the unit!
		// If you add it to the unit, it will disappear instantly when the unit is freed.
		GetTree().Root.AddChild(textNode);
		base.Die();
	}


	public void Exit()
	{
		if (_floatingTextScene == null) return;

		var textNode = _floatingTextScene.Instantiate<FloatingText>();

		// Set the text
		textNode.Text = $"-${_moneyDeducted}\n" + $"-${_hpDeducted} hp";
		textNode.AddThemeColorOverride("font_color", Colors.Red);

		textNode.GlobalPosition = GlobalPosition;

		GetTree().Root.AddChild(textNode);

		RemoveSelf();
	}

	public int GetMoneyDropped()
	{
		return _moneyDropped + _moneyModifier;
	}

	public int GetDistanceToExit()
	{
		int distance = 0;
		foreach (Command command in _commandQueue)
		{
			if (command is AttackMove attackMove)
			{
				distance++;
			}
		}
		return distance;
	}
}

