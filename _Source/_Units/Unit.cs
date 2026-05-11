using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class Unit : CharacterBody2D
{
	protected UnitPathfinder _pathfinder;

	private Sprite2D _selectionVisual;

	protected TextureProgressBar _healthBar;

	public BaseWeapon _weapon;

	public int _teamId;

	public bool _aiControlled = false;

	[Export] public float LeashDistance = 200f;

	[Export] public float _moveSpeed;

	[Export] public int _hpMax;


	[Export]
	private Array<EffectResource> _startingEffects;

	public int _hp { get; private set; }

	[Export] public string _name;

	[Export]
	protected float _radius = 100f;

	[Export]
	public int DebugTeamId
	{
		get => _teamId;
		set { }
	}

	[Export]
	public State DebugState
	{
		get => _state;
		set { }
	}

	[Signal]
	public delegate void DiedEventHandler(Unit unit);

	[Signal]
	public delegate void HpChangeEventHandler(Unit unit, int change);

	[Signal]
	public delegate void NewEffectEventHandler(Unit unit, Effect effect);

	[Signal]
	public delegate void BeginAttackEventHandler(Unit unit, Unit target);

	[Signal]
	public delegate void StopAttackEventHandler(Unit unit, Unit target);

	[Signal]
	public delegate void HitEnemyEventHandler(Unit unit, Unit target);

	[Signal]
	public delegate void UpdateInfoEventHandler(Unit unit);

	public enum State
	{
		Idle, 
		Attacking,
		Moving,
	}

	protected State _state;
	public List<Command> _commandQueue { get; protected set; } = [];
	public Command _currentCommand { get; protected set; }

	public bool _displayAttackRange;
	public float _attackRange;

	private float _speedModifier;

	public List<EffectResource> _effects = [];

	protected CollisionShape2D _attackCollisionShape;

	protected Unit _attackTarget;

	protected bool _active = true;

	public override void _Ready()
	{
		SetWeapon();
		SetSelectionVisual();
		SetAttackRange();
		SetPathFinder();
		SetHealthBar();
		SetInitialCommand();
		SetStartingEffects();
		SetSize();
	}

	protected void SetSize()
	{
		Sprite2D sprite = GetNode<Sprite2D>("MainSprite");
		Utils.ScaleVisualToRadius(sprite, _radius);
		CollisionShape2D collision = GetNode<CollisionShape2D>("CollisionShape2D");
		CircleShape2D collisionCircle = new CircleShape2D();
		collisionCircle.Radius = _radius;
		collision.Shape = collisionCircle;
	}

	protected void SetStartingEffects()
	{
		foreach (var effect in _startingEffects)
		{
			AddEffect(effect);
		}
	}

	public void AddEffect(EffectResource resource)
	{
		EffectResource resourceCopy = (EffectResource)resource.Duplicate();
		_effects.Add(resourceCopy);
		Effect node = EffectManager.Apply(resourceCopy, this);
		node.ConnectSignals(this);
		EmitSignal(SignalName.NewEffect, node);
		EmitSignal(SignalName.UpdateInfo);

		// Use this to make effects with the same name merge
		//if (!_effects.Any(e => e._effectName == resource._effectName))
		//{
		//	_effects.Add((EffectResource)resource.Duplicate());
		//	Effect node = EffectManager.Apply(resource, this);
		//	node.ConnectSignals(this);
		//	EmitSignal(SignalName.NewEffect, node);
		//	EmitSignal(SignalName.UpdateInfo);
		//}
		//else
		//{
		//	EffectResource oldEffect = _effects.First(e => e._effectName == resource._effectName);
		//	_effects.Remove(oldEffect);
		//	resource.MergeWithOld(oldEffect);
		//	AddEffect(resource);
		//}
	}

	public EffectResource GetEffect(Type type)
	{
		return _effects.First(e => e.GetType() == type);
	}

	protected void SetWeapon()
	{
		if (HasNode("WeaponComponent"))
		{
			_weapon = GetNode<BaseWeapon>("WeaponComponent");
		}
		else
		{
			_weapon = null;
		}
	}

	protected void SetSelectionVisual()
	{
		_selectionVisual = GetNode<Sprite2D>("SelectionCircle");
	}

	protected virtual void SetAttackRange()
	{
		if (_weapon is null)
		{
			return;
		}
		_attackCollisionShape = GetNode<CollisionShape2D>("AttackArea/AttackAreaCollision");
		_attackRange = _weapon.GetRange();

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

	protected void SetPathFinder()
	{
		_pathfinder = GetNode<UnitPathfinder>("UnitPathfinder");
		_pathfinder.SetSpeed(_moveSpeed + _speedModifier);
		_pathfinder.SetTeamId(_teamId);
	}

	protected void SetHealthBar()
	{
		_healthBar = GetNode<TextureProgressBar>("HealthBar");
		_hp = _hpMax;
		_healthBar.MaxValue = _hpMax;
		_healthBar.Value = _hp;
		UpdateHealthBar(_hpMax, _hpMax);
	}

	protected void SetInitialCommand()
	{
		_currentCommand = new NoCommand(this);
	}

	public void SetSelectionVisible(bool b)
	{
		_selectionVisual.Visible = b;
		if (b)
		{
			DisplayAttackRange();
		}
		else
		{
			HideAttackRange();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_active)
		{
			return;
		}
		if (_pathfinder._snapToTarget)
		{
			_pathfinder.CheckSnapToTarget(delta);
		}
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
			CheckTargetAlive();
		}

		if (_currentCommand is ForceAttack forceAttack)
		{
			if (!forceAttack.CheckFinish())
			{
				_pathfinder.SetTargetPosition(forceAttack._targetUnit.GlobalPosition);
			}
		}
		else if (_currentCommand is AggroedAttackMove aggroedAttackMove)
		{
			if (!aggroedAttackMove.CheckFinish())
			{
				_pathfinder.SetTargetPosition(aggroedAttackMove._targetUnit.GlobalPosition);
			}
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

	protected virtual void ProcessForceMove(ForceMove forceMove)
	{
		_currentCommand = forceMove;
		StopAttackingTarget();
		_pathfinder.SetTargetPosition(forceMove._targetLocation);
	}

	protected virtual void ProcessForceAttack(ForceAttack forceAttack)
	{
		_currentCommand = forceAttack;
		StopAttackingTarget();
		if (IsTargetInRange(forceAttack._targetUnit))
		{
			BeginAttackingTarget(forceAttack._targetUnit);
		}
		else
		{
			_pathfinder.SetTargetPosition(forceAttack._targetUnit.GlobalPosition);
		}	
	}

	protected virtual void ProcessAttackMove(AttackMove attackMove)
	{
		_currentCommand = attackMove;
		ScanForEnemies();
		_pathfinder.SetTargetPosition(attackMove._targetLocation);
	}

	protected virtual void ProcessAggroedAttackMove(AggroedAttackMove aggroedAttackMove)
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

	public void InsertCommand(int n, Command command)
	{
		_commandQueue.Insert(n, command);
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

	protected void BeginAttackingTarget(Unit unit)
	{
		if (_state == State.Attacking)
		{
			throw new Exception("Already attacking a target!");
		}
		_state = State.Attacking;
		_attackTarget = unit;
		_weapon?.BeginAttackingTarget(unit);
		if (_weapon is not null)
		{
			EmitSignal(SignalName.BeginAttack, unit);
		}
	}

	protected void StopAttackingTarget()
	{
		Unit oldTarget = _attackTarget;
		_state = State.Idle;
		_attackTarget = null;
		_weapon?.StopAttackingTarget();
		if (_weapon is not null)
		{
			EmitSignal(SignalName.StopAttack, oldTarget);
		}
	}

	public void OnHitEnemy(Unit enemy)
	{
		EmitSignal(SignalName.HitEnemy, enemy);
	}

	public void Hit(int damage, Unit source)
	{
		IncreaseHp(-damage);
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

	public void IncreaseHp(int change)
	{
		_hp += change;
		_hp = Math.Min(_hp, _hpMax);
		UpdateHealthBar(_hp, _hpMax);
		EmitSignal(SignalName.HpChange, change);
		EmitSignal(SignalName.UpdateInfo);
	}

	public virtual void Retaliate(Unit unit)
	{
		if (unit.CollisionLayer != UnitManager.UnitLayerMask)
		{
			return;
		}
		if (_currentCommand is AttackMove || _currentCommand is NoCommand)
		{
			AddCommand(new AggroedAttackMove(this, GlobalPosition, unit));
			AddCommand(_currentCommand);
			ProcessNextCommand();
		}
	}

	protected virtual void OnScanAreaBodyEntered(Node2D body)
	{
		//if (!_active)
		//{
		//	return;
		//}
		//if (body is Unit unit)
		//{
		//	// Check if the body is in the enemy group and we don't have a target yet
		//	if (unit._teamId != _teamId && _state != State.Attacking && (_currentCommand is NoCommand || _currentCommand is AttackMove || _currentCommand is AggroedAttackMove))
		//	{
		//		BeginAttackingTarget(unit);
		//	}
		//	else if (_currentCommand is ForceAttack forceAttack && forceAttack._targetUnit == body)
		//	{
		//		BeginAttackingTarget(unit);
		//	}
		//}
		//else
		//{
		//	return;
		//}
	}

	protected virtual void OnScanAreaBodyLeft(Node2D body)
	{
		if (!_active)
		{
			return;
		}
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
		if (_weapon is null)
		{
			return;
		}
		var scanArea = GetNode<Area2D>("AttackArea");

		// Get all overlapping physics bodies
		var bodies = scanArea.GetOverlappingBodies();
		if (!bodies.Any(body => body is InvaderUnit))
		{
			return;
		}

		var sortedBodies = bodies
				.OrderBy(body => GlobalPosition.DistanceSquaredTo(body.GlobalPosition))
				.ToList();

		//GD.Print("List: ");
		//foreach (Node2D body in sortedBodies)
		//{
		//	float dist = GlobalPosition.DistanceSquaredTo(body.GlobalPosition);
		//	GD.Print($"Body: {body.Name}, DistSquared: {dist}");
		//}

		for (int i = 0; i < sortedBodies.Count; i++)
		{
			var body = sortedBodies[i];
			if (body is Unit unit)
			{
				if (unit._teamId != _teamId && (_currentCommand is NoCommand || _currentCommand is AttackMove || _currentCommand is AggroedAttackMove))
				{
					BeginAttackingTarget(unit);
					break;
				}
			}
		}
	}

	protected void CheckTargetAlive()
	{
		if (!IsInstanceValid(_attackTarget))
		{
			StopAttackingTarget();
			ScanForEnemies();
		}
	}

	protected bool IsTargetInRange(Unit target)
	{
		var scanArea = GetNode<Area2D>("AttackArea");

		var bodies = scanArea.GetOverlappingBodies();
		foreach (Node2D body in bodies)
		{
			if (body == target)
			{
				return true;
			}
		}
		return false;
	}

	public void SetWeaponModifier(int damageModifier)
	{
		_weapon._damageModifier = damageModifier;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void IncreaseWeaponModifier(int change)
	{
		SetWeaponModifier(_weapon._damageModifier + change);
	}

	public void SetWeaponRangeModifier(float range)
	{
		_weapon._rangeModifier = range;
		SetAttackRange();
		EmitSignal(SignalName.UpdateInfo);
	}

	public void IncreaseWeaponRangeModifier(float change)
	{
		SetWeaponRangeModifier(_weapon._rangeModifier + change);
	}

	public void SetSpeedModifier(float speed)
	{
		if (this is StationaryUnit)
		{
			return;
		}
		_speedModifier = speed;
		SetPathFinder();
		EmitSignal(SignalName.UpdateInfo);
	}

	public void IncreaseSpeedModifier(float change)
	{
		SetSpeedModifier(_speedModifier + change);
	}

	public void SetAttackSpeedModifier(double speed)
	{
		_weapon._attackSpeedModifier = speed;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void IncreaseAttackSpeedModifier(double change)
	{
		SetAttackSpeedModifier(_weapon._attackSpeedModifier + change);
	}

	public void SetAttackDelayModifier(double speed)
	{
		_weapon._attackDelayModifier = speed;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void IncreaseAttackDelayModifier(double change)
	{
		SetAttackDelayModifier(_weapon._attackDelayModifier + change);
	}

	private void UpdateHealthBar(float currentHp, float maxHp)
	{
		float healthPercent = currentHp / maxHp;
		_healthBar.Value = currentHp;

		// Transition from Green (0.33) to Red (0.0) using HSV
		// Or use a simple Lerp between two specific colors:
		Color healthyColor = new Color(0, 1, 0, 0.75f);
		Color criticalColor = new Color(1, 0, 0, 0.75f);

		// This blends the two colors based on the health percentage
		_healthBar.Modulate = criticalColor.Lerp(healthyColor, healthPercent);
	}

	public void Exit()
	{
		Die();
	}

	protected virtual void Die()
	{
		SetProcess(false);
		SetPhysicsProcess(false);

		// 2. Disable collisions so other units don't bump into a corpse
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);

		EmitSignal(SignalName.Died, this);

		QueueFree();
	}

	

	public override void _Draw()
	{
		if (_displayAttackRange)
		{
			// Get the radius from your CollisionShape2D's resource
			var attackAreaShape = GetNode<CollisionShape2D>("AttackArea/AttackAreaCollision").Shape as CircleShape2D;

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

	public void DisablePhysicsProcess()
	{
		_active = false;
	}

	public void EnablePhysicsProcess()
	{
		_active = true;
	}

	public UnitPathfinder GetPathfinder()
	{
		return _pathfinder;
	}

	public Texture2D GetIconTexture()
	{
		return GetNode<Sprite2D>("MainSprite").Texture;
	}
}
