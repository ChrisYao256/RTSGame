using Godot;
using Godot.Collections;
using RTSGame._Source.Units;
using RTSGame.Units;
using System;
using System.Collections.Generic;

public partial class BuffNearbyUnits : Effect
{
	BuffNearbyUnitsResource _resource;
	Array<InvaderUnit> _affectedUnits = [];

	public BuffNearbyUnits(BuffNearbyUnitsResource resource) : base(resource)
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
		area.CollisionMask = UnitManager.UnitLayerMask;

		area.BodyEntered += OnBodyEntered;
		area.BodyExited += OnBodyExited;

		AddChild(area);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is InvaderUnit invader && invader != _parentUnit)
		{
			foreach (EffectResource resource in _resource._buffs)
			{
				invader.AddEffect(resource);
				_affectedUnits.Add(invader);
			}
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is InvaderUnit invader && _affectedUnits.Contains(invader))
		{
			foreach (EffectResource resource in _resource._buffs)
			{
				if (resource is IStackable stackableResource)
				{
					EffectResource negativeResource = stackableResource.MultiplyEffect(-1);
					invader.AddEffect(negativeResource);
					_affectedUnits.Remove(invader);
				}
				else
				{
					throw new Exception("a debuff must be an IStackable");
				}
			}
		}
	}
}
