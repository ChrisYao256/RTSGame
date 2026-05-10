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

	[Export] private bool _isExplosive = false;
	[Export] private float _explosionRadius;
	[Export] private int _explosionDamage;
	[Export] public PackedScene ExplosionVisualScene;

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

		if (!_isExplosive)
		{
			dealDamage = new Action<Unit, Projectile>((Unit enemy, Projectile projectile) =>
			{
				enemy.Hit(_damage, _parent);
				projectile.QueueFree();
			});
		}
		else
		{
			CollisionShape2D explosionCollision = new CollisionShape2D();
			CircleShape2D explosionCircle = new CircleShape2D();
			explosionCircle.Radius = _explosionRadius;
			explosionCollision.Shape = explosionCircle;

			Area2D explosionArea = new Area2D();
			explosionArea.CollisionMask = 3;
			explosionArea.AddChild(explosionCollision);

			dealDamage = new Action<Unit, Projectile>((Unit enemy, Projectile projectile) =>
			{
				enemy.Hit(_damage, _parent);

				var spaceState = enemy.GetWorld2D().DirectSpaceState;

				// 2. Setup the query parameters
				var query = new PhysicsShapeQueryParameters2D();

				// Use the shape from your existing explosionArea
				query.Shape = explosionCircle;

				// Place the query at the impact point
				query.Transform = new Transform2D(0, enemy.GlobalPosition);

				// Optional: Set collision mask to only hit units (e.g., Layer 2)
				// query.CollisionMask = 2; 

				// 3. Execute the query
				var results = spaceState.IntersectShape(query);

				foreach (var result in results)
				{
					var collider = result["collider"].As<Node2D>();

					if (collider is Unit unit && unit != enemy && unit._teamId != _parent._teamId)
					{
						unit.Hit(_explosionDamage, _parent);
					}
				}
				projectile.QueueFree();
				if (ExplosionVisualScene != null)
				{
					// Create the visual instance
					var viz = ExplosionVisualScene.Instantiate<Node2D>();

					// Add it to the world (Level), NOT the projectile
					GetTree().Root.AddChild(viz);

					// Move it to where the hit happened
					viz.GlobalPosition = enemy.Position;

					// If you want to scale the sprite to match the radius:
					float radius = _explosionRadius; // Get this from your shape
					Utils.ScaleVisualToRadius(viz, radius);
				}
			});
		}

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