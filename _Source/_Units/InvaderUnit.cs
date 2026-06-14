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
	private Vector4I _moneyDropped;
	[Export] 
	private PackedScene _floatingTextScene;
	[Export]
	public int _hpDeducted = 1;
	[Export]
	public Vector4I _moneyDeducted = new Vector4I();

	public Vector4I _moneyModifier;

	public override void _Ready()
	{
		base._Ready();
		_aiControlled = true;
	}

	public void SetMoneyModifier(Vector4I money)
	{
		_moneyModifier = money;
	}

	public void IncreaseMoneyModifier(Vector4I change)
	{
		_moneyModifier += change;
	}

	protected override void Die()
	{
		if (_floatingTextScene == null) return;

		var textNode = _floatingTextScene.Instantiate<FloatingText>();
		textNode.BbcodeEnabled = true;
		textNode.FitContent = true;

		// Set the text
		textNode.Text = "+" + Utils.MakeMoneyText(GetMoneyDropped());

		// Set the position to the unit's current global position
		textNode.GlobalPosition = GlobalPosition;

		// VERY IMPORTANT: Add it to the world, not the unit!
		// If you add it to the unit, it will disappear instantly when the unit is freed.
		GetTree().Root.AddChild(textNode);

		textNode.StartFloatingAnimation();

		base.Die();
	}


	public void Exit()
	{
		if (_floatingTextScene == null) return;

		var textNode = _floatingTextScene.Instantiate<FloatingText>();
		textNode.BbcodeEnabled = true;
		textNode.FitContent = true;

		// Set the text
		textNode.Text = $"-" + Utils.MakeMoneyText(_moneyDeducted) + $"-${_hpDeducted} hp";

		textNode.GlobalPosition = GlobalPosition;

		GetTree().Root.AddChild(textNode);

		textNode.StartFloatingAnimation();

		RemoveSelf();
	}

	public Vector4I GetMoneyDropped()
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

