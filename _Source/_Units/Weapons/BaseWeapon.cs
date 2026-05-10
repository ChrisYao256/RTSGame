using Godot;
using RTSGame.Units;
using System;

public abstract partial class BaseWeapon : Node2D
{
	[Export] public int _damage;
	[Export] public float _range;

	[Export] public double _attackCooldown = 1.0;
	[Export] public double _attackDelayLow = 0.1;
	[Export] public double _attackDelayHigh = 0.2;

	protected double _attackTimer = 0;

	protected Unit _parent;
	public Unit _attackTarget;

	public override void _Ready()
	{
		_parent = GetParent<Unit>();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_attackTarget is not null)
		{
			_attackTimer -= delta;
			if (_attackTimer < 0)
			{
				_attackTimer = _attackCooldown;
				PerformAttack(_attackTarget, _damage);
			}
		}
	}

	public virtual void BeginAttackingTarget(Unit target)
	{
		_attackTarget = target;
		_attackTimer = GD.RandRange(_attackDelayLow, _attackDelayHigh);
	}

	// Abstract method: Every attacker must define HOW they hit
	public abstract void PerformAttack(Unit target, int damage);

	public virtual float GetDPS()
	{
		return _damage / (float)_attackCooldown;
	}
}