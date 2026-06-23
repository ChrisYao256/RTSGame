using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class Blink : Effect
{
	BlinkResource _resource;
	int _counter;

	public Blink(BlinkResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.IsHit, Callable.From<Unit>(OnIsHit));

	}

	protected override void OnIsHit(Unit unit)
	{
		if (_counter == 0)
		{
			_counter++;
			TriggerBlink(_resource._distance);
		}
	}

	public void TriggerBlink(float distance)
	{
		if (_parentUnit is InvaderUnit invader)
		{
			if (invader._pathToExit.Count == 0) return;

			Vector2 currentPos = GlobalPosition;
			float distanceLeftToBlink = distance;
			Vector2 targetBlinkPosition = currentPos;

			// Start evaluating from the point we are currently walking towards

			while (distanceLeftToBlink > 0 && 0 < invader._pathToExit.Count)
			{
				Vector2 nextCorner = invader._pathToExit[0];
				float distanceToCorner = currentPos.DistanceTo(nextCorner);

				if (distanceLeftToBlink <= distanceToCorner)
				{
					// The blink ends on this path segment
					Vector2 direction = currentPos.DirectionTo(nextCorner);
					targetBlinkPosition = currentPos + (direction * distanceLeftToBlink);
					distanceLeftToBlink = 0;
					_parentUnit.AddCommand(new AttackMove(_parentUnit, nextCorner));
				}
				else
				{
					// The blink overshoots this waypoint! Jump to it, and consume distance
					distanceLeftToBlink -= distanceToCorner;
					currentPos = nextCorner;
					targetBlinkPosition = nextCorner;

					invader._pathToExit.RemoveAt(0);
				}
			}

			Line2D tracerLine = new Line2D();
			// Ensure the line has 2 points to work with
			tracerLine.ClearPoints();
			tracerLine.AddPoint(GlobalPosition);
			tracerLine.AddPoint(targetBlinkPosition);
			tracerLine.DefaultColor = _resource._tracerColor;
			tracerLine.Width = _resource._tracerWidth;

			GetTree().Root.AddChild(tracerLine);

			Timer tracerTimer = new Timer();
			tracerTimer.Timeout += () => tracerLine.Visible = false;

			GetTree().Root.AddChild(tracerTimer);

			tracerTimer.Start(0.1f);

			invader.GlobalPosition = targetBlinkPosition;

			_parentUnit.ProcessNextCommand();
		}
	}
}
