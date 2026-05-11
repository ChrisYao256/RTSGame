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
	public override void _Ready()
	{
		base._Ready();
	}

	public override void PerformAttack(Unit target, int d)
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
			projectile.QueueFree();
		});

		Projectile projectile = new(_parent._teamId, _projectileSpeed, _projectileTexture, _lifeTime, _projectileRadius, dealDamage, targetAngle);
		AddChild(projectile);
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

	public Projectile(int teamId, float speed, Texture2D texture, double lifeTime,  float projectileRadius, Action<Unit, Projectile> contact, float targetAngle, Action timeOut = null)
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
			
		_targetAngle = targetAngle;
		_contactAction = contact;
	}

	public override void _Ready()
	{
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
		if (_hasHit)
		{
			return;
		}
		if (body is Unit unit && unit._teamId != _teamId)
		{
			_hasHit = true;
			_contactAction.Invoke(unit, this);
		}
	}
}