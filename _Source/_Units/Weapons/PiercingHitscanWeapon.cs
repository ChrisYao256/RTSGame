using Godot;
using RTSGame.Units;
using System;
using Godot.Collections;

public partial class PiercingHitscanWeapon : BaseWeapon
{
	[Export] public double _tracerDuration = 0.1f;
	[Export] public Color _tracerColor = ThemePalette.Red;
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
		_tracerLine.Width = _tracerWidth * 0.8f;
		_tracerTimer = GetNode<Timer>("TracerLine/Timer");
		_tracerTimer.OneShot = true;
		_tracerTimer.Timeout += () => _tracerLine.Visible = false;
		base._Ready();
	}

	public override void PerformAttack(Unit target, int d)
	{
		_tracerLine.Visible = true;

		

		Vector2 delta = target.GlobalPosition - GlobalPosition;

		Vector2 rayDir = delta.Normalized();

		Vector2 farEnd = GlobalPosition + rayDir * 10000f;

		// Set Line2D points: start at A, stretch far along ray

		_tracerLine.SetPointPosition(0, GlobalPosition);
		_tracerLine.SetPointPosition(1, farEnd);

		// Start (or restart) the timer for the set duration
		_tracerTimer.Start(_tracerDuration);

		Action<Unit> dealDamage = new Action<Unit>((Unit enemy) =>
		{
			_parent.OnBeforeHitEnemy(enemy);
			enemy.Hit(GetDamage(), _parent);
			_parent.OnHitEnemy(enemy);
		});

		CollisionLine collisionLine = new CollisionLine(_parent._teamId, new(0,0), farEnd - GlobalPosition, _tracerWidth, _tracerDuration, dealDamage);
		AddChild(collisionLine);
		_parent.EmitSignal(Unit.SignalName.ShotFired);
	}
}

public partial class CollisionLine : Area2D
{
	private double _lifeTime;
	private Action<Unit> _contactAction;
	private Action _timeOut;
	private int _teamId;

	private Vector2 _point1;
	private Vector2 _point2;
	private float _width; // Added width backing field

	// Added a 'width' parameter to the constructor
	public CollisionLine(int teamId, Vector2 point1, Vector2 point2, float width, double lifeTime, Action<Unit> contact, Action timeOut = null)
	{
		_teamId = teamId;
		_lifeTime = lifeTime;
		_timeOut = timeOut ?? (() => QueueFree());

		_point1 = point1;
		_point2 = point2;
		_width = width;
		_contactAction = contact;
	}

	public override void _Ready()
	{
		// 1. Configure the area layers/masks
		CollisionMask = 3;
		BodyEntered += OnBodyEntered;

		// 2. Calculate the geometry data
		Vector2 delta = _point2 - _point1;
		float length = delta.Length();
		Vector2 midpoint = _point1 + (delta / 2f);
		float angle = delta.Angle();

		// 3. Create a thick physical shape
		CollisionShape2D collisionShape = new CollisionShape2D();
		CapsuleShape2D capsule = new CapsuleShape2D();

		// In Godot, a Capsule's height is the total end-to-end length.
		// Its radius is half of your line's width.
		capsule.Radius = _width / 2f;
		capsule.Height = length;

		collisionShape.Shape = capsule;

		// 4. Orient and place the shape to match the line vector
		collisionShape.Position = midpoint;
		collisionShape.Rotation = angle + Mathf.Pi / 2f; // Offset by 90 degrees because Godot capsules point up vertically by default

		AddChild(collisionShape);

		// 5. Timer handle
		Timer timer = new Timer();
		timer.WaitTime = _lifeTime;
		timer.Timeout += () => QueueFree();
		timer.Timeout += _timeOut;
		AddChild(timer);
		timer.Start();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Unit unit && unit._teamId != _teamId)
		{
			_contactAction.Invoke(unit);
		}
	}
}