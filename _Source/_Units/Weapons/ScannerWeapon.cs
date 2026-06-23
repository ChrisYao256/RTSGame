using Godot;
using RTSGame.Units;
using System;

public partial class ScannerWeapon : BaseWeapon
{
	[Export] public Color _tracerColor = ThemePalette.Blue;
	[Export] public float _tracerWidth = 6.0f;

	private Line2D _tracerLine;
	private Timer _tracerTimer;

	private bool _scanning = false;

	public override void _Ready()
	{
		_tracerLine = GetNode<Line2D>("TracerLine");
		// Ensure the line has 2 points to work with
		_tracerLine.ClearPoints();
		_tracerLine.AddPoint(Vector2.Zero);
		_tracerLine.AddPoint(Vector2.Zero);
		_tracerLine.DefaultColor = _tracerColor;
		_tracerLine.Width = _tracerWidth;
		_tracerTimer = GetNode<Timer>("TracerLine/Timer");
		_tracerTimer.Timeout += () =>
		{
			_parent.OnBeforeHitEnemy(_attackTarget);
			_attackTarget.Hit(GetDamage(), _parent);
			_parent.OnHitEnemy(_attackTarget);
			_tracerLine.Visible = false;
			_scanning = false;
		};
		base._Ready();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_attackTarget is not null)
		{
			if (!_scanning)
			{
				_attackTimer -= delta;
				if (_attackTimer < 0)
				{
					if (_useAttackDelay)
					{
						_attackTimer = GetAttackDelay();
					}
					PerformAttack(_attackTarget, GetDamage());
				}
			}
			else
			{
				_tracerLine.SetPointPosition(0, GlobalPosition);
				_tracerLine.SetPointPosition(1, _attackTarget.GlobalPosition);
			}
		}
		else
		{
			StopAttackingTarget();
		}
	}

	public override void PerformAttack(Unit target, int d)
	{
		_scanning = true;
		_tracerLine.Visible = true;

		_tracerLine.SetPointPosition(0, GlobalPosition);
		_tracerLine.SetPointPosition(1, target.GlobalPosition);

		_tracerTimer.Start(_attackCooldown);

		// Start (or restart) the timer for the set duration
		_tracerTimer.Start();
	}

	public override void StopAttackingTarget()
	{
		_tracerTimer.Stop();
		_tracerLine.Visible = false;
		_scanning = false;
		_attackTarget = null;
	}

	public override float GetDPS()
	{
		return (GetDamage()) / (float)(GetCooldown() + GetAttackDelay());
	}
}