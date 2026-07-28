using Godot;
using RTSGame.Units;
using Godot.Collections;

public partial class ZoneWeapon : BaseWeapon
{
  [Export]
  public Shape2D _shape;

  [Export]
  PackedScene _attackVisualEffect;

	public override void _Ready()
	{
		base._Ready();
		_firePoint = GetParent().GetNode("TurretTurner").GetNode<Marker2D>("Marker2D");
	}

	public override void SetAttackRange(float range)
	{
		if (_shape is CircleShape2D circleShape)
		{
			circleShape.Radius = range;
		}
	}

	public override void PerformAttack(Unit target, int d)
	{
		float angleToTarget = GlobalPosition.AngleToPoint(target.GlobalPosition);
		Transform2D queryTransform = new Transform2D(angleToTarget, GlobalPosition);

		if (_shape is CircleShape2D circleShape)
		{
			if (_attackVisualEffect != null)
			{
				// Create the visual instance
				var viz = _attackVisualEffect.Instantiate<Node2D>();

				// Add it to the world (Level), NOT the projectile
				GetTree().Root.AddChild(viz);

				// Move it to where the hit happened
				viz.GlobalPosition = _firePoint.GlobalPosition;


				// If you want to scale the sprite to match the radius:
				float radius = circleShape.Radius; // Get this from your shape
				Utils.ScaleVisualToRadius(viz.GetNode<AnimatedSprite2D>("Sprite2D"), radius / 1.2f);
			}
		}
		else
		{
			var visualizer = new DynamicShapeVisualizer();
			visualizer.SetShape(_shape);
			Timer timer = new Timer();
			timer.Timeout += (() => { visualizer.QueueFree(); });
			timer.WaitTime = 0.2;
			visualizer.AddChild(timer);
			GetTree().Root.AddChild(visualizer);
			visualizer.Transform = queryTransform;
			visualizer.GlobalPosition = _firePoint.GlobalPosition;
			timer.Start();
		}

		// 1. Get the direct space state for the current 2D world
		PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;

    // 3. Configure the query parameters
    var query = new PhysicsShapeQueryParameters2D();
    query.Shape = _shape;
    query.Transform = queryTransform; // Centers the query at this node's position
    query.CollisionMask = UnitManager.UnitLayerMask; // Only search on specific layers
    query.CollideWithBodies = true;     // Detect CharacterBody2D, RigidBody2D, etc.
    query.CollideWithAreas = false;     // Ignore Area2D zones if not needed

    Array<Dictionary> results = spaceState.IntersectShape(query);

    // 5. Parse the results
    foreach (Dictionary result in results)
    {
      // The "collider" key holds the reference to the object (Node2D/PhysicsBody2D)
      if (result.TryGetValue("collider", out Variant colliderVariant))
      {
        Node2D entity = colliderVariant.As<Node2D>();

        // Example: If you use an interface for your entities
        if (entity is InvaderUnit invader)
        {
					DamageContext context = new DamageContext(_parent, invader, d, DamageType.DirectAttack);

					_parent.OnBeforeHitEnemy(context);
					invader.Hit(context);
					_parent.OnHitEnemy(invader);
				}
      }
    }
  }
}