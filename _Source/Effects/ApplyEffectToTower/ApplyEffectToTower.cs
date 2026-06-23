using Godot;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

public partial class ApplyEffectToTower : Effect
{
	ApplyEffectToTowerResource _resource;

	Timer _timer;
	Line2D _tracerLine;
	Timer _tracerTimer;

	public ApplyEffectToTower(ApplyEffectToTowerResource resource) : base(resource)
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
		_tracerLine = new Line2D();
		_tracerLine.AddPoint(Vector2.Zero);
		_tracerLine.AddPoint(Vector2.Zero);
		_tracerLine.DefaultColor = _resource._tracerColor;
		_tracerLine.Width = _resource._tracerWidth;
		_tracerLine.TopLevel = true;
		_tracerTimer = new Timer();
		_tracerTimer.OneShot = true;
		_tracerTimer.Timeout += () => {
			_tracerLine.Visible = false;
		};
	  AddChild(_tracerLine);
		AddChild(_tracerTimer);

		_timer = new Timer();
		_timer.Name = "Timer";
		AddChild(_timer);
		_timer.WaitTime = _resource._interval;
		_timer.OneShot = false;
		_timer.Timeout += () =>
		{
			List<Node> nodes = Utils.QueryPhysicsCircle(GetWorld2D(), GlobalPosition, _resource._range, 4);

			List<Unit> towers = [];
			foreach (Node node in nodes)
			{
				if (node is TowerUnit tower && tower._towerType == TowerUnit.TowerType.Defense && tower._teamId != _parentUnit._teamId)
				{
					towers.Add(tower);
				}
			}

			if (towers.Count == 0)
			{
				return;
			}

			_parentUnit.PauseNavigation();
			GetTree().CreateTimer(0.5f).Timeout += () => _parentUnit.ResumeNavigation();

			Random random = new Random();
			int index = random.Next(towers.Count);
			foreach (EffectResource effect in _resource._debuffs)
			{
				towers[index].AddEffect(effect);
			}

			MakeVisualEffectLine(towers[index].GlobalPosition);
		};
		_timer.Start();
	}

	private void MakeVisualEffectLine(Vector2 endPoint)
	{
		_tracerLine.SetPointPosition(0, GlobalPosition);
		_tracerLine.SetPointPosition(1, endPoint);
		_tracerLine.Visible = true;
		_tracerTimer.Start(_resource._tracerDuration);
	}
}