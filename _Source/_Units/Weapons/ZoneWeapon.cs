using Godot;
using RTSGame.Units;
using Godot.Collections;

public partial class ZoneWeapon : BaseWeapon
{
  [Export]
  public Shape2D _shape;

  [Export]
  PackedScene _attackVisualEffect;

  public override void PerformAttack(Unit target, int d)
  {
		if (_attackVisualEffect != null)
		{
			// Instantiate the blueprint into a live node
			Node2D visualInstance = _attackVisualEffect.Instantiate<Node2D>();

			// Move it to the strike location and add it to the game world
			visualInstance.GlobalPosition = target.GlobalPosition;
			GetTree().CurrentScene.AddChild(visualInstance);

			// (Note: The scene should handle its own animation and QueueFree() when done!)
		}

		var visualizer = new DynamicShapeVisualizer();
		visualizer.SetShape(_shape);
    Timer timer = new Timer();
    timer.Timeout += (() => { visualizer.QueueFree(); });
    timer.WaitTime = 0.2;
    visualizer.AddChild(timer);
    GetTree().Root.AddChild(visualizer);
   
		timer.Start();

		float angleToTarget = GlobalPosition.AngleToPoint(target.GlobalPosition);
		Transform2D queryTransform = new Transform2D(angleToTarget, GlobalPosition);

    visualizer.Transform = queryTransform;

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
          invader.Hit(GetDamage(), _parent);
					_parent.OnHitEnemy(invader);
				}
      }
    }
  }
}