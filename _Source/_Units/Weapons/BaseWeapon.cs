using Godot;
using RTSGame.Units;
using System;

public abstract partial class BaseWeapon : Node2D
{
	[Export] private int _damage;
	[Export] private float _range;

	[Export] private double _attackCooldown = 1.0;
	[Export] private double _attackDelayLow = 0.1;
	[Export] private double _attackDelayHigh = 0.2;

	[Export] protected bool _useAttackDelay = false;

	protected double _attackTimer = 0;

	protected Unit _parent;
	public Unit _attackTarget;

	public int _damageModifier = 0;
	public float _rangeModifier = 0;
	public double _attackSpeedModifier = 0;
	public double _attackDelayModifier = 0;

	public override void _Ready()
	{
		_parent = GetParent<Unit>();
		_attackTimer = _attackCooldown;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_attackTarget is not null)
		{
			_attackTimer -= delta;
			if (_attackTimer < 0)
			{
				_attackTimer = _attackCooldown / ( 1 + _attackSpeedModifier);
				PerformAttack(_attackTarget, _damage + _damageModifier);
			}
		}
	}

	public virtual void BeginAttackingTarget(Unit target)
	{
		if (_attackTarget != null)
		{
			throw new Exception("Already have a target!");
		}
		_attackTarget = target;
		if (_useAttackDelay)
		{
			_attackTimer = GetAttackDelay();
		}
	}

	public virtual void StopAttackingTarget()
	{
		_attackTarget = null;
	}

	// Abstract method: Every attacker must define HOW they hit
	public abstract void PerformAttack(Unit target, int damage);
	
	public float GetRange()
	{
		return _range + _rangeModifier;
	}

	public int GetDamage()
	{
		return _damage + _damageModifier;
	}

	public double GetCooldown()
	{
		return _attackCooldown / (1 + _attackSpeedModifier);
	}

	public double GetAttackDelay()
	{
		return GD.RandRange(_attackDelayLow / (1 + _attackDelayModifier), _attackDelayHigh / (1 + _attackDelayModifier));
	}

	public virtual float GetDPS()
	{
		return (_damage + _damageModifier) / (float) (_attackCooldown/ (1 + _attackSpeedModifier));
	}
}