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

	public override void SetAttackRange()
	{
		_attackCollisionShape = GetNode<CollisionShape2D>("AttackArea/AttackAreaCollision");
		_attackRange = TDManager.TileSize / 2f;
		if (_attackCollisionShape.Shape is CircleShape2D circle)
		{
			circle = (CircleShape2D)circle.Duplicate();
			circle.Radius = _attackRange;
			if (IsInstanceValid(_attackCollisionShape))
			{
				_attackCollisionShape.Shape = circle;
			}
			//Callable.From(() => {
			//	if (IsInstanceValid(_attackCollisionShape))
			//	{
			//		_attackCollisionShape.Shape = circle;
			//	}
			//}).CallDeferred();
		}
		else
		{
			throw new Exception("Attack area shape is not a disk");
		}
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
		if (unit is not InvaderUnit)
		{
			throw new Exception("Unit is not InvaderUnit!");
		}
		((InvaderUnit)unit).Exit();
		_tdManager.UnitExited((InvaderUnit)unit);
	}
}