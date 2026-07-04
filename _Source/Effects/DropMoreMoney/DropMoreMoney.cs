using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class DropMoreMoney : Effect
{
	DropMoreMoneyResource _firstResource;

	List<(DropMoreMoneyResource, Timer)> _debuffs = [];

	private Timer _longestTimer;

	public DropMoreMoney(DropMoreMoneyResource resource) : base(resource)
	{
		_firstResource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.Died, Callable.From(OnUnitDied));
		OnCreation();
		AddResource(_firstResource);
	}

	public void AddResource(DropMoreMoneyResource newResource)
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

	protected override void OnUnitDied()
	{
		if (_firstResource._source != null)
		{
			_firstResource._source.IncreaseMoneyGainedStat(((InvaderUnit)_parentUnit)._moneyTempModifier);
		}
	}

	public void RecalculateDebuff()
	{
		float maxIncrease = 0;
		foreach (var e in _debuffs)
		{
			if (e.Item1._percentIncrease > maxIncrease)
			{
				maxIncrease = e.Item1._percentIncrease;
				_firstResource._source = e.Item1._source;
			}
		}
		_firstResource._percentIncrease = maxIncrease;
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

			((InvaderUnit)_parentUnit).SetMoneyTempModifier(maxIncrease);
		if (maxIncrease == 0)
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
