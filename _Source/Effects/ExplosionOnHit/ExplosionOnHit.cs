using Godot;
using RTSGame.Units;
using System;

public partial class ExplosionOnHit : Effect
{
	ExplosionOnHitResource _resource;
	//public SelfHealing(int healAmount, double healInterval)
	//{
	//	_healAmount = healAmount;
	//	_healInterval = healInterval;
	//}

	public ExplosionOnHit(ExplosionOnHitResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		_parentUnit = unit;
		unit.Connect(Unit.SignalName.HitEnemy, Callable.From<Unit>(OnHitEnemy));
	}

	protected override void OnHitEnemy(Unit enemy)
	{
		CollisionShape2D explosionCollision = new CollisionShape2D();
		CircleShape2D explosionCircle = new CircleShape2D();
		explosionCircle.Radius = _resource._explosionRadius;
		explosionCollision.Shape = explosionCircle;

		Area2D explosionArea = new Area2D();
		explosionArea.CollisionMask = 3;
		explosionArea.AddChild(explosionCollision);

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

			if (collider is Unit unit && unit != enemy && unit._teamId != _parentUnit._teamId)
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
			viz.GlobalPosition = enemy.Position;

			// If you want to scale the sprite to match the radius:
			float radius = _resource._explosionRadius; // Get this from your shape
			Utils.ScaleVisualToRadius(viz, radius);
		}
	}
}