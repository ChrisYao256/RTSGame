using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class Unit : CharacterBody2D
{
	private UnitPathfinder _pathfinder;

	private Sprite2D _selectionVisual;

	private TextureProgressBar _healthBar;

	private BaseWeapon _weapon;

	public int _teamId;

	[Export] public float LeashDistance = 500f;

	[Export] public bool _isBuilding;
	[Export] public float _moveSpeed;

	[Export] public int _hpMax;
	public int _hp { get; private set; }

	[Export] public string _name;

	[Export]
	public int DebugTeamId
	{
		get => _teamId;
		set { }
	}

	[Export]
	public int DebugHp
	{
		get => _teamId;
		set { }
	}

	[Export]
	public string DebugCommand
	{
		get => _currentCommand.GetType().ToString();
		set { }
	}

	[Export]
	public State DebugState
	{
		get => _state;
		set { }
	}

	public enum State
	{
		Idle, 
		Attacking,
		Moving,
	}

	private State _state;
	public List<Command> _commandQueue { get; private set; } = [];
	public Command _currentCommand { get; private set; }

	public bool _displayAttackRange;
	public float _attackRange;

	private CollisionShape2D _attackCollisionShape;

	public event Action<Unit> Died;

	private Unit _attackTarget;

	public override void _Ready()
	{
		GD.Print($"{Name} initialized.");
		_weapon = GetNode<BaseWeapon>("WeaponComponent");
		_pathfinder = GetNode<UnitPathfinder>("UnitPathfinder");
		_pathfinder.SetSpeed(_moveSpeed);
		_pathfinder.SetTeamId(_teamId);

		_selectionVisual = GetNode<Sprite2D>("SelectionCircle");

		_attackCollisionShape = GetNode<CollisionShape2D>("AttackArea/AttackAreaCollision");
		SetAttackRange(_weapon._range);

		_healthBar = GetNode<TextureProgressBar>("HealthBar");
		_hp = _hpMax;
		_healthBar.MaxValue = _hpMax;
		_healthBar.Value = _hp;
		UpdateHealthBar(_hpMax, _hpMax);

		ProcessNextCommand();
		
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

	public void SetSelectionVisible(bool b)
	{
		_selectionVisual.Visible = b;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_pathfinder.IsNavigationFinished())
		{
			_currentCommand.CheckFinish();
		}
		else if (_state != State.Attacking)
		{
			_state = State.Moving;
			_pathfinder.ProcessMovement(delta);
		}

		if (_state == State.Attacking)
		{
			// switch target if unit is died
			if (!IsInstanceValid(_attackTarget))
			{
				StopAttackingTarget();
				ScanForEnemies();
			}
		}

		if (_currentCommand is ForceAttack forceAttack)
		{
			_pathfinder.SetTargetPosition(forceAttack._targetUnit.GlobalPosition);
			_currentCommand.CheckFinish();
		}
		else if (_currentCommand is AggroedAttackMove aggroedAttackMove)
		{
			_pathfinder.SetTargetPosition(aggroedAttackMove._targetUnit.GlobalPosition);
			_currentCommand.CheckFinish();
		}
		else if (_currentCommand is NoCommand)
		{
			ProcessNextCommand();
		}
	}
	public void ProcessNextCommand()
	{
		Command command = new NoCommand(this);
		if (_commandQueue.Count > 0)
		{
			command = _commandQueue[0];
			_commandQueue.RemoveAt(0);
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

	private void ProcessForceMove(ForceMove forceMove)
	{
		_currentCommand = forceMove;
		StopAttackingTarget();
		_pathfinder.SetTargetPosition(forceMove._targetLocation);
	}

	private void ProcessForceAttack(ForceAttack forceAttack)
	{
		_currentCommand = forceAttack;
		StopAttackingTarget();
		_pathfinder.SetTargetPosition(forceAttack._targetUnit.GlobalPosition);
	}

	private void ProcessAttackMove(AttackMove attackMove)
	{
		_currentCommand = attackMove;
		ScanForEnemies();
		_pathfinder.SetTargetPosition(attackMove._targetLocation);
	}

	private void ProcessAggroedAttackMove(AggroedAttackMove aggroedAttackMove)
	{
		_currentCommand = aggroedAttackMove;
		ScanForEnemies();
		_pathfinder.SetTargetPosition(aggroedAttackMove._targetUnit.GlobalPosition);
	}

	public void ClearAllCommands()
	{
		_commandQueue = new List<Command>();
		ProcessNextCommand();
	}

	public void AddCommand(Command command)
	{
		_commandQueue.Add(command);
	}

	//private void Chase(Unit unit)
	//{
	//	if (unit._teamId != _teamId)
	//	{
	//		_pathfinder.SetChaseTarget(unit);
	//	}
	//}

	//private void StopMoving()
	//{
	//	_pathfinder.ForceFinishNavigation();
	//}

	private void BeginAttackingTarget(Unit unit)
	{
		_state = State.Attacking;
		_attackTarget = unit;
		_weapon.BeginAttackingTarget(unit);
	}

	private void StopAttackingTarget()
	{
		_state = State.Idle;
		_attackTarget = null;
		_weapon.BeginAttackingTarget(null);
	}

	public void Hit(int damage, Unit source)
	{
		_hp -= damage;
		UpdateHealthBar(_hp, _hpMax);

		Area2D socialArea = GetNode<Area2D>("AidArea");
		var nearbyBodies = socialArea.GetOverlappingBodies();

		foreach (var body in nearbyBodies)
		{
			// Check if the body is a Unit and on the same team
			if (body is Unit ally && ally._teamId == this._teamId)
			{
				ally.Retaliate(source);
			}
		}

		if (_hp <= 0)
		{
			Die();
		}
	}

	public void Retaliate(Unit unit)
	{
		if (_currentCommand is AttackMove || _currentCommand is NoCommand)
		{
			AddCommand(_currentCommand);
			AddCommand(new AggroedAttackMove(this, GlobalPosition, unit));
			ProcessNextCommand();
		}
	}

	private void OnScanAreaBodyEntered(Node2D body)
	{
		if (body is Unit unit)
		{
			// Check if the body is in the enemy group and we don't have a target yet
			if (unit._teamId != _teamId && _state != State.Attacking &&(_currentCommand is NoCommand || _currentCommand is AttackMove || _currentCommand is AggroedAttackMove))
			{
				BeginAttackingTarget(unit);
			}
			else if (_currentCommand is ForceAttack forceAttack && forceAttack._targetUnit == body)
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
			StopAttackingTarget();
		}
		else
		{
			return;
		}
	}

	public void ScanForEnemies()
	{
		if (_state == State.Attacking)
		{
			return;
		}
		var scanArea = GetNode<Area2D>("AttackArea");

		// Get all overlapping physics bodies
		var bodies = scanArea.GetOverlappingBodies();

		foreach (Node2D body in bodies)
		{
			if (body is Unit unit)
			{
				if (unit._teamId != _teamId && (_currentCommand is NoCommand || _currentCommand is AttackMove || _currentCommand is AggroedAttackMove))
				{
					BeginAttackingTarget(unit);
				}
			}
		}
	}

	private void UpdateHealthBar(float currentHp, float maxHp)
	{
		float healthPercent = currentHp / maxHp;
		_healthBar.Value = currentHp;

		// Transition from Green (0.33) to Red (0.0) using HSV
		// Or use a simple Lerp between two specific colors:
		Color healthyColor = Colors.Green;
		Color criticalColor = Colors.Red;

		// This blends the two colors based on the health percentage
		_healthBar.Modulate = criticalColor.Lerp(healthyColor, healthPercent);
	}

	private void Die()
	{
		SetProcess(false);
		SetPhysicsProcess(false);

		// 2. Disable collisions so other units don't bump into a corpse
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);

		Died?.Invoke(this);
		QueueFree();
	}

	

	public override void _Draw()
	{
		if (_displayAttackRange)
		{
			// Get the radius from your CollisionShape2D's resource
			var attackAreaShape = GetNode<CollisionShape2D>("UnitPathfinder/AttackArea/AttackAreaCollision").Shape as CircleShape2D;

			if (attackAreaShape != null)
			{
				Color drawColor = new Color(0.2f, 0.6f, 1.0f, 0.3f); // Light blue, semi-transparent
				DrawCircle(Vector2.Zero, attackAreaShape.Radius, drawColor);
			}
		}
	}

	public void DisplayAttackRange()
	{
		_displayAttackRange = true;
		UpdateVisualRange();
	}

	public void HideAttackRange()
	{
		_displayAttackRange = false;
		UpdateVisualRange();
	}

	// Call this whenever the range changes to update the visual
	public void UpdateVisualRange()
	{
		QueueRedraw();
	}
	public UnitPathfinder GetPathfinder()
	{
		return _pathfinder;
	}
}
