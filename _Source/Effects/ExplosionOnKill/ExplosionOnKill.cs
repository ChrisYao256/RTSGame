using Godot;
using RTSGame.Units;
using System;

public partial class ExplosionOnKill : Effect
{
	ExplosionOnKillResource _resource;

	int _hitCounter = 0;

	public ExplosionOnKill(ExplosionOnKillResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.KilledUnit, Callable.From<Unit>(OnKilledEnemy));
	}

	protected override void OnKilledEnemy(Unit enemy)
	{
		Timer timer = new Timer();
		timer.WaitTime = 0.2f;
		AddChild(timer);
		timer.OneShot = true;
		var spaceState = enemy.GetWorld2D().DirectSpaceState;
		Vector2 enemyPosition = enemy.GlobalPosition;
		timer.Timeout += () =>
		{
			CircleShape2D explosionCircle = new CircleShape2D();
			explosionCircle.Radius = _resource._explosionRadius;

			

			// 2. Setup the query parameters
			var query = new PhysicsShapeQueryParameters2D();

			// Use the shape from your existing explosionArea
			query.Shape = explosionCircle;

			// Place the query at the impact point
			query.Transform = new Transform2D(0, enemyPosition);

			// Optional: Set collision mask to only hit units (e.g., Layer 2)
			// query.CollisionMask = 2; 

			// 3. Execute the query
			var results = spaceState.IntersectShape(query);

			foreach (var result in results)
			{
				var collider = result["collider"].As<Node2D>();

				if (collider is Unit unit && unit._teamId != _parentUnit._teamId)
				{
					unit.Hit(_resource._explosionDamage, _parentUnit);
				}
			}
			if (_resource.ExplosionVisualScene != null)
			{
				// Create the visual instance
				var viz = _resource.ExplosionVisualScene.Instantiate<Node2D>();

				// Add it to the world (Level), NOT the projectile
				GetTree().Root.AddChild(viz);

				// Move it to where the hit happened
				viz.GlobalPosition = enemyPosition;

				// If you want to scale the sprite to match the radius:
				float radius = _resource._explosionRadius; // Get this from your shape
				Utils.ScaleVisualToRadius(viz.GetNode<Sprite2D>("Sprite2D"), radius);
			}
			timer.Free();
		};
		timer.Start();
	}
}