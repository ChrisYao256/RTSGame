using Godot;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class Exit : StationaryUnit
{
	public TDManager _tdManager;

	protected override void SetAttackRange()
	{
		_attackCollisionShape = GetNode<CollisionShape2D>("AttackArea/AttackAreaCollision");
		_attackRange = 50f;
	}

	public override void _PhysicsProcess(double delta)
	{
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

	protected override void OnScanAreaBodyEntered(Node2D body)
	{
		if (body is Unit unit)
		{
			// Check if the body is in the enemy group and we don't have a target yet
			if (unit._teamId != _teamId)
			{
				UnitExited(unit);
			}
		}
		else
		{
			return;
		}
	}

	protected override void OnScanAreaBodyLeft(Node2D body)
	{
		return;
	}

	public void UnitExited(Unit unit)
	{
		unit.Exit();
		_tdManager.UnitExited(unit);
	}
}