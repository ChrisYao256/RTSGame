using Godot;
using RTSGame.Source;
using Godot.Collections;
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
	public Vector4I _moneyTempModifier;

	public Array<Vector2> _pathToExit;

	public TDManager _tdManager;

	public override void _Ready()
	{
		base._Ready();
		_aiControlled = true;
		_tdManager = GetTree().CurrentScene.GetNode<TDManager>("TdManager");
	}

	public override void ProcessNextCommand()
	{
		Command command = new NoCommand(this);
		if (_commandQueue.Count > 0)
		{
			command = _commandQueue[0];
			_commandQueue.RemoveAt(0);
		}
		else if (_pathToExit.Count > 0)
		{
			command = new AttackMove(this, _pathToExit[0]);
			_pathToExit.RemoveAt(0);
		}
		else
		{
			_currentCommand = command;
		}
		if (command is AttackMove attackMove)
		{
			ProcessAttackMove(attackMove);
		}
		else if (command is ForceMove forceMove)
		{
			ProcessForceMove(forceMove);
		}
		else if (command is ForceAttack forceAttack)
		{
			ProcessForceAttack(forceAttack);
		}
		else if (command is AggroedAttackMove aggroedAttackMove)
		{
			ProcessAggroedAttackMove(aggroedAttackMove);
		}
		else if (command is NoCommand noCommand)
		{
			ScanForEnemies();
		}
	}

	public void SetMoneyModifier(Vector4I money)
	{
		_moneyModifier = money;
	}

	public void IncreaseMoneyModifier(Vector4I change)
	{
		_moneyModifier += change;
	}

	public void SetMoneyTempModifier(Vector4I money)
	{
		_moneyTempModifier = money;
	}

	protected override void Die()
	{
		if (_floatingTextScene != null && GetMoneyDropped() != new Vector4I(0, 0, 0, 0))
		{
			_currentFloatingAnimationCount++;

			var textNode = _floatingTextScene.Instantiate<FloatingText>();
			textNode.BbcodeEnabled = true;
			textNode.FitContent = true;

			if (_moneyTempModifier != new Vector4I(0, 0, 0, 0))
			{
				textNode.Text = "+" + Utils.MakeMoneyText(GetNormalMoneyDropped()) + "\n+bonus " + Utils.MakeMoneyText(_moneyTempModifier);
			}
			else
			{
				textNode.Text = "+" + Utils.MakeMoneyText(GetNormalMoneyDropped());
			}


			// Set the position to the unit's current global position
			textNode.GlobalPosition = GlobalPosition + new Vector2(0, 30) * (_currentFloatingAnimationCount - 1);

			// VERY IMPORTANT: Add it to the world, not the unit!
			// If you add it to the unit, it will disappear instantly when the unit is freed.
			GetTree().Root.AddChild(textNode);

			Timer timer = new();
			timer.Timeout += () =>
			{
				_currentFloatingAnimationCount--;
				timer.QueueFree();
			};
			timer.OneShot = true;
			GetTree().Root.AddChild(timer);
			timer.Start(1f);

			textNode.StartFloatingAnimation();
		}
		base.Die();
	}


	public void Exit()
	{
		if (_floatingTextScene == null) return;

		var textNode = _floatingTextScene.Instantiate<FloatingText>();
		textNode.BbcodeEnabled = true;
		textNode.FitContent = true;

		// Set the text
		textNode.Text = Utils.MakeMoneyText(_moneyDeducted) + $"\n -{_hpDeducted} hp";

		textNode.GlobalPosition = GlobalPosition;

		GetTree().Root.AddChild(textNode);

		textNode.StartFloatingAnimation();

		RemoveSelf();
	}

	public Vector4I GetMoneyDropped()
	{
		return _moneyDropped + _moneyModifier + _moneyTempModifier;
	}

	public Vector4I GetNormalMoneyDropped()
	{
		return _moneyDropped + _moneyModifier;
	}

	public float GetDistanceToExit()
	{
		if (_pathToExit is null)
		{
			return 9999;
		}
		float distance = 0;
		Vector2 oldPos = GlobalPosition;
		foreach (Vector2 pos in _pathToExit)
		{
			distance += pos.DistanceTo(oldPos);
			oldPos = pos;
		}
		return distance;
	}
}

