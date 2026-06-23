using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class SlowMovement : Effect
{
	SlowMovementResource _firstResource;

	List<(SlowMovementResource, Timer)> _debuffs = [];

	private Timer _longestTimer;

	public SlowMovement(SlowMovementResource resource) : base(resource)
	{
		_firstResource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
		AddResource(_firstResource);
	}

	public void AddResource(SlowMovementResource newResource)
	{
		Timer timer = new Timer();
		AddChild(timer);
		timer.WaitTime = newResource._time;
		timer.OneShot = true;
		timer.Start();
		timer.Timeout += (() => {
			_debuffs.Remove((newResource, timer));
			RecalculateDebuff();
		});
		_debuffs.Add((newResource, timer));
		RecalculateDebuff();
	}

	protected override void UpdateTempDebuffIcon(UpgradeButton button)
	{
		if (_longestTimer is null)
		{
			return;
		}
		button.UpdateAffordabilityDisplay((float)_longestTimer.TimeLeft / _firstResource._time);
	}

	public void RecalculateDebuff()
	{
		float maxReduction = 0;
		foreach (var e in _debuffs)
		{
			if (e.Item1._percentDecrease > maxReduction)
			{
				maxReduction = e.Item1._percentDecrease;
			}
		}
		_firstResource._percentDecrease = maxReduction;
		_firstResource.SetDescription();
		_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);

		float maxDuration = 0;
		foreach (var e in _debuffs)
		{
			if (e.Item2.TimeLeft > maxDuration)
			{
				maxDuration = (float)e.Item2.TimeLeft;
			}
		}
		if (_longestTimer is null)
		{
			_longestTimer = new Timer();
			_longestTimer.OneShot = true;
			AddChild(_longestTimer);
			_longestTimer.Start(maxDuration);
		}
		else if (maxDuration > _longestTimer.TimeLeft)
		{
			_longestTimer.QueueFree();
			_longestTimer = new Timer();
			_longestTimer.OneShot = true;
			AddChild(_longestTimer);
			_longestTimer.Start(maxDuration);
		}

		_parentUnit.SetSpeedDebuff(maxReduction);
		if (maxReduction == 0)
		{
			RemoveEffectResource();
			RemoveEffectNode();
			_parentUnit.Modulate = new Color(1, 1, 1);
			_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		}
		else
		{
			_parentUnit.Modulate = ThemePalette.Blue;
		}
	}
}
