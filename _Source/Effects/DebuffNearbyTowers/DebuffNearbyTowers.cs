using RTSGame.Units;
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class DebuffNearbyTowers : Effect
{
	private Timer _timer;
	DebuffNearbyTowersResource _resource;
	//public SelfHealing(int healAmount, double healInterval)
	//{
	//	_healAmount = healAmount;
	//	_healInterval = healInterval;
	//}

	public DebuffNearbyTowers(DebuffNearbyTowersResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
	}

	protected override void OnCreation()
	{
		_timer = new Timer();
		AddChild(_timer);
		_timer.WaitTime = _resource._period;
		_timer.OneShot = false;
		_timer.Timeout += () => {
			List<Node> nodes = Utils.QueryPhysicsCircle(GetWorld2D(), GlobalPosition, _resource._radius, 4);

			List<Unit> towers = [];
			foreach (Node node in nodes)
			{
				if (node is TowerUnit tower)
				{
					towers.Add(tower);
				}
			}
			foreach (TowerUnit tower in towers)
			{
				tower.AddEffect(_resource._debuff);
			}
			if (_resource._slowVisualScene != null)
			{
				// Create the visual instance
				var viz = _resource._slowVisualScene.Instantiate<Node2D>();

				// Add it to the world (Level), NOT the projectile
				GetTree().Root.AddChild(viz);

				// Move it to where the hit happened
				viz.GlobalPosition = GlobalPosition;

				// If you want to scale the sprite to match the radius:
				float radius = _resource._radius; // Get this from your shape
				Utils.ScaleVisualToRadius(viz.GetNode<Sprite2D>("Sprite2D"), radius);
			}
		};
		_timer.Start();
	}
}
