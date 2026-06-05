using RTSGame.Units;
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class NearbyHealing : Effect
{
	private Timer _timer;
	NearbyHealingResource _resource;
	//public SelfHealing(int healAmount, double healInterval)
	//{
	//	_healAmount = healAmount;
	//	_healInterval = healInterval;
	//}

	public NearbyHealing(NearbyHealingResource resource) : base(resource)
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
		_timer.WaitTime = _resource._healInterval;
		_timer.OneShot = false;
		_timer.Timeout += () => {
			_parentUnit.PauseNavigation();
			GetTree().CreateTimer(0.5f).Timeout += () => _parentUnit.ResumeNavigation();
			List<Node> nodes = Utils.QueryPhysicsCircle(GetWorld2D(), GlobalPosition, _resource._radius);

			List<Unit> friendlies = [];
			foreach (Node node in nodes)
			{
				if (node is Unit unit && unit._teamId == _parentUnit._teamId)
				{
					friendlies.Add(unit);
				}
			}
			if (_resource._isShield)
			{
				foreach (Unit unit in friendlies)
				{
					unit.IncreaseShield(_resource._healAmount);
				}
			}
			else
			{
				foreach (Unit unit in friendlies)
				{
					unit.IncreaseHp(_resource._healAmount, false);
				}
			}
			if (_resource._healingVisualScene != null)
			{
				// Create the visual instance
				var viz = _resource._healingVisualScene.Instantiate<Node2D>();

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
