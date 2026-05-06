using Godot;
using System;

namespace RTSGame.Units;

public partial class UnitPathfinder : Node
{
	public const float DesiredDistance = 10f;

	private CharacterBody2D _parentBody;

	private NavigationAgent2D _navAgent;


	private float _speed;
	private int _teamId;

	private float _stagnationTimer = 0;
	private float _checkInterval = 0.2f;
	private Vector2 _positionLastCheck;

	public override void _Ready()
	{
		_parentBody = GetParent<CharacterBody2D>();
		_navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		_navAgent.TargetDesiredDistance = DesiredDistance;

		// These signals help handle pathfinding logic automatically
		_navAgent.VelocityComputed += OnVelocityComputed;
	}

	public void SetTeamId(int id)
	{
		_teamId = id;
	}

	public void SetSpeed(float speed)
	{
		_speed = speed;
	}

	//public void SetMoveTarget(Vector2 target)
	//{
	//	StopAttackingTarget();
	//	_state = State.ForceMoving;
	//	_navAgent.TargetPosition = target;
	//}

	//public void SetAttackMoveTarget(Vector2 target)
	//{
	//	_state = State.AttackMoving;
	//	_navAgent.TargetPosition = target;
	//	ScanForEnemies();
	//}

	//public void SetForceAttackTarget(Unit unit)
	//{
	//	StopAttackingTarget();
	//	_state = State.ForceAttacking;
	//	_chaseTarget = unit;
	//	_navAgent.TargetPosition = unit.GlobalPosition;
	//	ScanForTarget(unit);
	//}

	//public void SetChaseTarget(Unit unit)
	//{
	//	_state = State.Chasing;
	//	_chaseTarget = unit;
	//	_navAgent.TargetPosition = unit.GlobalPosition;
	//	ScanForTarget(unit);
	//}

	//private void BeginAttackingTarget(Unit unit)
	//{
	//	if (_state == State.ForceAttacking)
	//	{
	//		_forceAttackQueued = true;
	//	}
	//	_state = State.Attacking;
	//	_attackTarget = unit;
	//	((Unit)_parentBody).BeginAttackingTarget(unit);
	//}

	//private void StopAttackingTarget()
	//{
	//	_state = State.Idle;
	//	_attackTarget = null;
	//	((Unit)_parentBody).BeginAttackingTarget(null);
	//}

	public void SetTargetPosition(Vector2 position)
	{
		_navAgent.TargetPosition = position;
	}

	public void SetTargetDesiredDistance(float distance)
	{
		_navAgent.TargetDesiredDistance = distance;
	}

	// The "Order" function called by the Manager
	public void ProcessMovement(double delta)
	{
		

		Vector2 nextPathPos = _navAgent.GetNextPathPosition();
		Vector2 direction = (nextPathPos - _parentBody.GlobalPosition).Normalized();
		Vector2 intendedVelocity = direction * _speed;

		var length = _navAgent.GetCurrentNavigationPath();

		if (_navAgent.AvoidanceEnabled)
			_navAgent.Velocity = intendedVelocity;
		else
			ApplyMovement(intendedVelocity);

		// Stagnation timer calculations
		//_stagnationTimer += (float)delta;

		//if (_stagnationTimer >= _checkInterval)
		//{
		//	// Calculate how far we actually moved since the last check
		//	float distanceMoved = _parentBody.GlobalPosition.DistanceTo(_positionLastCheck);

		//	// If we moved less than 2 pixels despite trying to move
		//	if (distanceMoved < 5.0f)
		//	{
		//		ForceFinishNavigation();
		//	}
		//	_positionLastCheck = _parentBody.GlobalPosition;
		//	_stagnationTimer = 0f;
		//}
	}

	public void ForceFinishNavigation()
	{
		_navAgent.Velocity = new(0, 0);
		_navAgent.TargetPosition = _parentBody.GlobalPosition;
	}

	private void FinishedNavigation()
	{
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

	public bool IsNavigationFinished()
	{
		return _navAgent.IsNavigationFinished();
	}
}
