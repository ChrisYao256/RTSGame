using Godot;
using RTSGame.Units;

public partial class HitscanWeapon : BaseWeapon
{
	[Export] public double _tracerDuration = 0.1f;
	[Export] public Color _tracerColor = ThemePalette.Yellow;
	[Export] public float _tracerWidth = 2.0f;

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
		_tracerTimer.OneShot = true;
		_tracerTimer.Timeout += () => _tracerLine.Visible = false;
		base._Ready();
	}

	public override void PerformAttack(Unit target, int d)
	{
		_tracerLine.Visible = true;

		_tracerLine.SetPointPosition(0, _firePoint.GlobalPosition);
		_tracerLine.SetPointPosition(1, target.GlobalPosition);

		// Start (or restart) the timer for the set duration
		_tracerTimer.Start(_tracerDuration);

		_parent.OnBeforeHitEnemy(target);
		target.Hit(GetDamage(), _parent);
		_parent.OnHitEnemy(target);
		_parent.EmitSignal(Unit.SignalName.ShotFired);
	}
}