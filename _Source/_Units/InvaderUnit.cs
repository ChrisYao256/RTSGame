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

	private Vector2 _pathOffset;
	private Array<Vector2> _pathToExit;

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

	public void SetRandomPathOffset()
	{
		_pathOffset = Grid.GetRandomOffset();
	}

	public void SetPathOffset(Vector2 offset)
	{
		_pathOffset = Grid.ClampOffset(offset);
	}

	public void SetPathToExit(Array<Vector2> path)
	{
		for (int i = 0; i < path.Count; i++)
		{
			path[i] += _pathOffset;
		}
		_pathToExit = path;
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

	public void SetMoneyTempModifier(float percentIncrease)
	{
		_moneyTempModifier = Utils.VectorScalarMultiplication(GetNormalMoneyDropped(), percentIncrease);
	}

	protected override void Die()
	{
		if (_floatingTextScene != null && GetSelfMoneyDropped() != new Vector4I(0, 0, 0, 0))
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

	/// <summary>
	/// Returns the money that will be dropped if this unit dies now. Used to actually award money for kills. Is equivalent ot GetNormalMoneyDropped() if the unit is not added to TD. 
	/// </summary>
	/// <returns></returns>
	public Vector4I GetSelfMoneyDropped()
	{
		return _moneyDropped + _moneyModifier + _moneyTempModifier;
	}

	/// <summary>
	/// Returns the money that will be dropped if this unit and units that it spawns die. Used to calculate total income for spawner towers. 
	/// </summary>
	/// <returns></returns>
	public Vector4I GetTotalMoneyDropped()
	{
		if (!_effects.Any(o => o.GetType() == typeof(SpawnUnitOnDeathResource)))
		{
			return GetSelfMoneyDropped();
		}
		else
		{
			Vector4I moneyFromSpawns = new(0,0,0,0);
			SpawnUnitOnDeathResource spawnEffectResource = (SpawnUnitOnDeathResource)_effects.First(o => o.GetType() == typeof(SpawnUnitOnDeathResource));
			foreach (InvaderStatsIncreaseResource unit in spawnEffectResource._spawnedUnits)
			{
				InvaderUnit invader = unit.GetInvader();
				moneyFromSpawns += invader.GetTotalMoneyDropped();
				invader.QueueFree();
			}
			return GetSelfMoneyDropped() + moneyFromSpawns;
		}
	}

	/// <summary>
	/// Returns the money that should be dropped without temporary modifiers such as Analyzed. Used to display bonus floating text when the unit dies. 
	/// </summary>
	/// <returns></returns>
	public Vector4I GetNormalMoneyDropped()
	{
		return _moneyDropped + _moneyModifier;
	}

	public Array<Vector2> GetPathToExit() => _pathToExit;

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

