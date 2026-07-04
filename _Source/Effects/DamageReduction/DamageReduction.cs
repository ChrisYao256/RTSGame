using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class DamageReduction : Effect
{
	DamageReductionResource _firstResource;

	List<(DamageReductionResource, Timer)> _debuffs = [];

	private Timer _longestTimer;

	public DamageReduction(DamageReductionResource resource) : base(resource)
	{
		_firstResource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
		AddResource(_firstResource);
	}

	public void AddResource(DamageReductionResource newResource)
	{
		if (newResource._time != -1)
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
		else
		{
			_debuffs.Add((newResource, null));
			RecalculateDebuff();
		}
	}

	protected override void UpdateTempDebuffIcon(UpgradeButton button)
	{
		if (_longestTimer is null)
		{
			button.UpdateAffordabilityDisplay(1f);
			return;
		}
		button.UpdateAffordabilityDisplay((float)_longestTimer.TimeLeft / _firstResource._time);
	}

	public void RecalculateDebuff()
	{
		// cancel out each negative resource with a positive resource if their numbers are opposite and their timers are both null(infinite time)
		foreach (var n in _debuffs)
		{
			if (n.Item1._percentDecrease < 0)
			{
				foreach (var e in _debuffs)
				{
					if (e.Item1._percentDecrease == -n.Item1._percentDecrease && e.Item2 == n.Item2 && e.Item2 is null)
					{
						e.Item1._percentDecrease = 0;
						n.Item1._percentDecrease = 0;
						break;
					}
				}
			}
		}

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

		// update _longestTimer for progress bar display
		float maxDuration = 0;
		foreach (var e in _debuffs)
		{
			if (e.Item2 is null)
			{
				continue;
			}
			if (e.Item2.TimeLeft > maxDuration)
			{
				maxDuration = (float)e.Item2.TimeLeft;
			}
		}
		if (maxDuration > 0)
		{
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
		}


		_parentUnit.SetDamageReductionBuff(maxReduction);
		if (maxReduction <= 0)
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
