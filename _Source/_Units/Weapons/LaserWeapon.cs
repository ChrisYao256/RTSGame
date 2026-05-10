using Godot;
using RTSGame.Units;

public partial class LaserWeapon : BaseWeapon
{
	[Export] public Color _tracerColor = Colors.Red;
	[Export] public float _tracerWidth = 2.0f;

	private Line2D _tracerLine;

	private float _damageRemainder;

	public override void _Ready()
	{
		_tracerLine = GetNode<Line2D>("TracerLine");
		// Ensure the line has 2 points to work with
		_tracerLine.ClearPoints();
		_tracerLine.AddPoint(Vector2.Zero);
		_tracerLine.AddPoint(Vector2.Zero);
		_tracerLine.DefaultColor = _tracerColor;
		_tracerLine.Width = _tracerWidth;
		base._Ready();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_attackTarget is not null)
		{
			_tracerLine.Visible = true;
			_tracerLine.SetPointPosition(0, GlobalPosition);
			_tracerLine.SetPointPosition(1, _attackTarget.GlobalPosition);

			float damageThisFrame = _damage * (float)delta + _damageRemainder;

			int damageToDeal = Mathf.FloorToInt(damageThisFrame);

			if (damageToDeal >= 1)
			{
				PerformAttack(_attackTarget, damageToDeal);
				// Keep the "leftover" fraction for the next frame
				_damageRemainder = damageThisFrame - damageToDeal;
			}
			else
			{
				_damageRemainder = damageThisFrame;
			}
		}
		else
		{
			_tracerLine.Visible = false;
		}
	}

	public override void PerformAttack(Unit target, int d)
	{
		target.Hit(d , _parent);
	}
}