using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class StationaryUnit : Unit
{
	public override void _Ready()
	{
		_moveSpeed = 0;
		SetWeapon();
		SetSelectionVisual();
		SetAttackRange();
		SetHealthBar();
		SetInitialCommand();
		SetStartingEffects();
		SetSize();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_active)
		{
			return;
		}
		if (_state == State.Attacking)
		{
			// switch target if unit is died
			CheckTargetAlive();
			if (!IsTargetInRange(_attackTarget))
			{
				StopAttackingTarget();
				ScanForEnemies();
			}
		}

		if (_currentCommand is NoCommand)
		{
			ProcessNextCommand();
		}
	}

	protected override void ProcessForceMove(ForceMove forceMove)
	{
		ProcessNextCommand();
	}

	protected override void ProcessForceAttack(ForceAttack forceAttack)
	{
		_currentCommand = forceAttack;
		StopAttackingTarget();
		if (IsTargetInRange(forceAttack._targetUnit))
		{
			BeginAttackingTarget(forceAttack._targetUnit);
		}
	}

	protected override void ProcessAttackMove(AttackMove attackMove)
	{
		ProcessNextCommand();
	}

	protected override void ProcessAggroedAttackMove(AggroedAttackMove aggroedAttackMove)
	{
		ProcessNextCommand();
	}

	public override void Retaliate(Unit unit)
	{
		return;
	}
}

