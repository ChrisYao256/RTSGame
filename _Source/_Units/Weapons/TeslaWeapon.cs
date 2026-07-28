using Godot;
using RTSGame.Units;
using System;
using System.Threading.Tasks;

public partial class TeslaWeapon : BaseWeapon
{
	[Export] public double _tracerDuration = 0.1f;
	[Export] public Color _tracerColor = ThemePalette.Green;
	[Export] public float _tracerWidth = 2.0f;
	[Export] public int _hitCount;
	[Export] public float _shotInterval;

	private Line2D _tracerLine;
	private Timer _tracerTimer;

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
		_tracerTimer.Timeout += () => _tracerLine.Visible = false;
		base._Ready();
	}

	public override void PerformAttack(Unit target, int d)
	{
		if (_attackTarget == null)
		{
			return;
		}
		ShootProjectiles(d);
	}

	private async void ShootProjectiles(int damage)
	{
		for (int i = 0; i < _hitCount; i++)
		{
			if (_attackTarget == null)
			{
				_parent.OnVolleyEnded();
				return;
			}

			Godot.Collections.Array<Unit> enemies = _parent.GetEnemiesInRange();

			if (enemies.Count == 0)
			{
				_parent.OnVolleyEnded();
				return;
			}

			Random random = new Random();
			int index = random.Next(0, enemies.Count);
			Unit target = enemies[index];

			_tracerLine.Visible = true;

			_tracerLine.SetPointPosition(0, _firePoint.GlobalPosition);
			_tracerLine.SetPointPosition(1, target.GlobalPosition);

			// Start (or restart) the timer for the set duration
			_tracerTimer.Start(_tracerDuration);

			DamageContext context = new DamageContext(_parent, target, damage, DamageType.DirectAttack);

			_parent.OnBeforeHitEnemy(context);
			target.Hit(context);
			_parent.OnHitEnemy(target);


			if (i < _hitCount - 1)
			{
				await ToSignal(GetTree().CreateTimer(_shotInterval, processAlways: false), SceneTreeTimer.SignalName.Timeout);
			}
		}
		_parent.OnVolleyEnded();
	}

	public override float GetDPS()
	{
		return (GetDamage() * _hitCount) / (float)(GetCooldown());
	}
}