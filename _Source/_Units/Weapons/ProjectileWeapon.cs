using Godot;
using RTSGame.Units;
using System;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

public partial class ProjectileWeapon : BaseWeapon
{
	[Export] protected float _projectileSpeed;
	[Export] protected Texture2D _projectileTexture;
	[Export] protected double _lifeTime;
	[Export] protected float _projectileRadius;
	[Export] public int _pierceCount;

	/// <summary>
	/// Delays the projectile after SignalName.ShotFired is emitted. Use to sync animation
	/// <summary>
	[Export] protected float _delayProjectile;
	
	private Marker2D _firePoint;

	public override void _Ready()
	{
		base._Ready();
		_firePoint = GetParent().GetNode("TurretTurner").GetNode<Marker2D>("Marker2D");
	}

	public override void PerformAttack(Unit target, int d)
	{
		Projectile projectile = SpawnProjectile(_firePoint.GlobalPosition);

		if (_delayProjectile > 0)
		{
			Timer timer = new Timer();
			timer.WaitTime = _delayProjectile;
			timer.Timeout += () => AddChild(projectile);
			timer.OneShot = true;
			AddChild(timer);
			timer.Start();
		}
		else
		{
			AddChild(projectile);
		}
		_parent.EmitSignal(Unit.SignalName.ShotFired);
	}

	protected Projectile SpawnProjectile(Vector2 spawnPosition)
	{
		Vector2 targetDir = _attackTarget.GlobalPosition - GlobalPosition;
		float targetAngle = targetDir.Angle();

		Action<Unit, Projectile> dealDamage = (Unit unit, Projectile projectile) =>
		{

		};

		dealDamage = new Action<Unit, Projectile>((Unit enemy, Projectile projectile) =>
		{
			enemy.Hit(GetDamage(), _parent);
			_parent.OnHitEnemy(enemy);
		});

		return new(_parent._teamId, spawnPosition, _projectileSpeed, _projectileTexture, _lifeTime, _projectileRadius, dealDamage, targetAngle, _pierceCount);

	}
}



public partial class Projectile: Area2D
{
	private float _projectileSpeed;
	private Texture2D _projectileTexture;
	private double _lifeTime;
	private float _projectileRadius;
	private Action<Unit, Projectile> _contactAction;
	private Action _timeOut;
	private float _targetAngle;
	private int _teamId;

	private bool _hasHit = false;

	private int _pierceCount = 1;
	private int _hasPierced = 0;

	private Vector2 _initialPosition;

	public Projectile(int teamId, Vector2 position, float speed, Texture2D texture, double lifeTime,  float projectileRadius, Action<Unit, Projectile> contact, float targetAngle, int pierceCount = 1, Action timeOut = null)
	{
		_teamId = teamId;
		_projectileSpeed = speed;
		_projectileTexture = texture;
		_lifeTime = lifeTime;
		_projectileRadius = projectileRadius;
		if (timeOut is not null)
		{
			_timeOut = timeOut;
		}
		else
		{
			_timeOut = (() => { QueueFree(); });
		}
		_initialPosition = position;
		_targetAngle = targetAngle;
		Rotation = _targetAngle;
		_pierceCount = pierceCount;
		_contactAction = contact;
	}

	public override void _Ready()
	{
		GlobalPosition = _initialPosition;

		CollisionMask = 3;
		BodyEntered += OnBodyEntered;

		CollisionShape2D shape = new CollisionShape2D();
		CircleShape2D circle = new CircleShape2D();
		circle.Radius = _projectileRadius;
		shape.Shape = circle;
		AddChild(shape);

		Sprite2D sprite = new Sprite2D();
		sprite.Texture = _projectileTexture;

		Vector2 textureSize = sprite.Texture.GetSize();

		// 2. Calculate the scale required to match the diameter
		// We use the larger dimension (Width or Height) to ensure it fits
		float maxDimension = Mathf.Max(textureSize.X, textureSize.Y);
		float targetScale = (_projectileRadius * 2.0f) / maxDimension;

		// 3. Apply the scale
		sprite.Scale = new Vector2(targetScale, targetScale);
		AddChild(sprite);

		Timer timer = new Timer();
		timer.WaitTime = _lifeTime;
		timer.Timeout += () => QueueFree();
		timer.Timeout += _timeOut;
		AddChild(timer);
		timer.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += Vector2.FromAngle(_targetAngle) * _projectileSpeed * (float)delta;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_hasPierced >= _pierceCount)
		{
			return;
		}
		if (body is Unit unit && unit._teamId != _teamId)
		{
			_hasPierced++;
			_contactAction.Invoke(unit, this);
			if (_hasPierced >= _pierceCount)
			{
				QueueFree();
			}
		}
	}
}