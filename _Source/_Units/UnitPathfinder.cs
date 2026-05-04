using Godot;
using System;

namespace RTSGame.Units;

public partial class UnitPathfinder : Node
{
	private CharacterBody2D _parentBody;

	private NavigationAgent2D _navAgent;

	private CollisionShape2D _attackCollisionShape;

	public enum State
	{
		Idle, // retaliate if attacked. Otherwise stationary
		Attacking, // attack a target. When the target leaves range, go to idle by default. Use _forceMoveQueued, _attackMoveQueued, _chaseQueued to change this behavior
		ForceMoving, // move to a position ignoring all attacks. Becomes idle when done
		AttackMoving, // move to a position but stop to attack anything in range. Will go back to attack moving after the target dies/leaves range. Retaliate if attacked. 
		ForceAttacking, // attack a chosen target. Will pursuit if target leaves. Ignores all other enemies while pursuing. 
		Following, 
		Chasing, // chasing an attacker. Will attack all other units in range. 
	}

	private float _speed;
	private float _attackRange;
	private int _teamId;
	private State _state = State.Idle;
	private bool _forceAttackQueued = false;
	private bool _attackMoveQueued = false;
	private bool _chaseQueued = false;

	private Unit _attackTarget;
	private Unit _chaseTarget;

	private float _stagnationTimer = 0;
	private float _checkInterval = 0.2f;
	private Vector2 _positionLastCheck;

	[Export] 
	public State DebugState {
		get => _state;
		set {}
	}

	public override void _Ready()
	{
		_parentBody = GetParent<CharacterBody2D>();
		_navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		_attackCollisionShape = GetNode<CollisionShape2D>("AttackArea/AttackAreaCollision");

		// These signals help handle pathfinding logic automatically
		_navAgent.VelocityComputed += OnVelocityComputed;
	}

	public void SetTeamId(int id)
	{
		_teamId = id;
	}

	public void SetAttackRange(float range)
	{
		_attackRange = range;

		// CRITICAL: Make the shape unique so changing this unit 
		// doesn't change every other unit of the same type.
		if (_attackCollisionShape.Shape is CircleShape2D circle)
		{
			circle = (CircleShape2D)circle.Duplicate();
			circle.Radius = _attackRange;
			_attackCollisionShape.Shape = circle;
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
			if (!IsInstanceValid(_attackTarget))
			{
				StopAttackingTarget();
				ScanForEnemies();
			}
		}
		else if (_state == State.ForceAttacking)
		{
			if (!IsInstanceValid(_chaseTarget))
			{
				StopAttackingTarget();
				_chaseTarget = null;
				ScanForEnemies();
			}
		}
	}

	public void SetSpeed(float speed)
	{
		_speed = speed;
	}

	public void SetMoveTarget(Vector2 target)
	{
		StopAttackingTarget();
		_state = State.ForceMoving;
		_navAgent.TargetPosition = target;
	}

	public void SetAttackMoveTarget(Vector2 target)
	{
		_state = State.AttackMoving;
		_navAgent.TargetPosition = target;
		ScanForEnemies();
	}

	public void SetForceAttackTarget(Unit unit)
	{
		StopAttackingTarget();
		_state = State.ForceAttacking;
		_chaseTarget = unit;
		_navAgent.TargetPosition = unit.GlobalPosition;
		ScanForTarget(unit);
	}

	public void SetChaseTarget(Unit unit)
	{
		_state = State.Chasing;
		_chaseTarget = unit;
		_navAgent.TargetPosition = unit.GlobalPosition;
		ScanForTarget(unit);
	}

	private void BeginAttackingTarget(Unit unit)
	{
		if (_state == State.ForceAttacking)
		{
			_forceAttackQueued = true;
		}
		_state = State.Attacking;
		_attackTarget = unit;
		((Unit)_parentBody).BeginAttackingTarget(unit);
	}

	private void StopAttackingTarget()
	{
		_state = State.Idle;
		_attackTarget = null;
		((Unit)_parentBody).BeginAttackingTarget(null);
	}

	private void OnScanAreaBodyEntered(Node2D body)
	{
		if (body is Unit unit)
		{
			// Check if the body is in the enemy group and we don't have a target yet
			if (unit._teamId != _teamId && (_state == State.Idle || _state == State.AttackMoving))
			{
				BeginAttackingTarget(unit);
			}
			else if (unit == _chaseTarget && _state == State.ForceAttacking)
			{
				BeginAttackingTarget(unit);
			}
		}
		else
		{
			return;
		}
	}

	private void OnScanAreaBodyLeft(Node2D body)
	{
		if (body is Unit unit && unit == _attackTarget)
		{
			if (!_forceAttackQueued)
			{
				StopAttackingTarget();
				ScanForEnemies();
			}
			else
			{
				SetForceAttackTarget(unit);
			}
		}
		else
		{
			return;
		}
	}

	// used at the end of force moving and when target leaves range
	public void ScanForEnemies()
	{
		var scanArea = GetNode<Area2D>("AttackArea");

		// Get all overlapping physics bodies
		var bodies = scanArea.GetOverlappingBodies();

		foreach (Node2D body in bodies)
		{
			if (body is Unit unit)
			{
				if (unit._teamId != _teamId && (_state == State.Idle || _state == State.AttackMoving))
				{
					BeginAttackingTarget(unit);
				}
			}
		}
	}

	public void ScanForTarget(Unit target)
	{
		var scanArea = GetNode<Area2D>("AttackArea");

		// Get all overlapping physics bodies
		var bodies = scanArea.GetOverlappingBodies();

		foreach (Node2D body in bodies)
		{
			if (body is Unit unit)
			{
				if (unit == target)
				{
					BeginAttackingTarget(target);
				}
			}
		}
	}

	// The "Order" function called by the Manager
	public void ProcessMovement(double delta)
	{
		if (_state == State.Attacking || _state == State.Idle) return;
		if (_navAgent.IsNavigationFinished()) { FinishedNavigation(); return; }
		if (_state == State.ForceAttacking)
		{
			_navAgent.TargetPosition = _chaseTarget.GlobalPosition;
		}

		Vector2 nextPathPos = _navAgent.GetNextPathPosition();
		Vector2 direction = (nextPathPos - _parentBody.GlobalPosition).Normalized();
		Vector2 intendedVelocity = direction * _speed;

		var length = _navAgent.GetCurrentNavigationPath();

		if (_navAgent.AvoidanceEnabled)
			_navAgent.Velocity = intendedVelocity;
		else
			ApplyMovement(intendedVelocity);

		// Stagnation timer calculations
		_stagnationTimer += (float)delta;

		if (_stagnationTimer >= _checkInterval)
		{
			// Calculate how far we actually moved since the last check
			float distanceMoved = _parentBody.GlobalPosition.DistanceTo(_positionLastCheck);

			// If we moved less than 2 pixels despite trying to move
			if (distanceMoved < 10.0f && _state != State.ForceAttacking)
			{
				ForceFinishNavigation();
			}
			_positionLastCheck = _parentBody.GlobalPosition;
			_stagnationTimer = 0f;
		}
	}

	public void ForceFinishNavigation()
	{
		_navAgent.Velocity = new(0, 0);
		SetMoveTarget(_parentBody.GlobalPosition);
	}

	private void FinishedNavigation()
	{
		_state = State.Idle;
		ScanForEnemies();
	}

	private void OnVelocityComputed(Vector2 safeVelocity)
	{
		//if (safeVelocity.Length() < 5.0f)
		//{
		//	safeVelocity = Vector2.Zero;
		//}

		ApplyMovement(safeVelocity);
	}

	private void ApplyMovement(Vector2 velocity)
	{
		_parentBody.Velocity = velocity;
		_parentBody.MoveAndSlide();
	}

	public State GetState()
	{
		return _state;
	}
}
