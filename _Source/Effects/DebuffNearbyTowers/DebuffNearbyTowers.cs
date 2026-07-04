using Godot;
using Godot.Collections;
using RTSGame._Source.Units;
using RTSGame.Units;
using System;
using System.Collections.Generic;

/// <summary>
/// applies an IStackable effect to towers that enter range. 
/// </summary>
public partial class DebuffNearbyTowers : Effect
{
	DebuffNearbyTowersResource _resource;
	Array<TowerUnit> _affectedTowers = [];

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
		if (_resource._slowVisualScene != null)
		{
			// Create the visual instance
			var viz = _resource._slowVisualScene.Instantiate<Node2D>();

			// Add it to the world (Level), NOT the projectile
			AddChild(viz);

			// Move it to where the hit happened
			viz.GlobalPosition = GlobalPosition;

			// If you want to scale the sprite to match the radius:
			float radius = _resource._radius; // Get this from your shape
			Utils.ScaleVisualToRadius(viz.GetNode<Sprite2D>("Sprite2D"), radius);
		}

		Area2D area = new();
		CollisionShape2D shape = new CollisionShape2D();
		CircleShape2D circle = new CircleShape2D();
		circle.Radius = _resource._radius;
		shape.Shape = circle;
		area.AddChild(shape);
		area.CollisionMask = UnitManager.TowerLayerMask;

		area.BodyEntered += OnBodyEntered;
		area.BodyExited += OnBodyExited;

		AddChild(area);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is TowerUnit tower && tower._towerType == TowerUnit.TowerType.Defense)
		{
			foreach (EffectResource resource in _resource._debuffs)
			{
				tower.AddEffect(resource);
				_affectedTowers.Add(tower);
			}
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is TowerUnit tower && _affectedTowers.Contains(tower))
		{
			foreach (EffectResource resource in _resource._debuffs)
			{
				if (resource is IStackable stackableResource)
				{
					EffectResource negativeResource = stackableResource.MultiplyEffect(-1);
					tower.AddEffect(negativeResource);
					_affectedTowers.Remove(tower);
				}
				else
				{
					throw new Exception("a debuff must be an IStackable");
				}
			}
		}
	}
}
