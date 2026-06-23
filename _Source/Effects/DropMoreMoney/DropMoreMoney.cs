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

	public void RecalculateDebuff()
	{
		Vector4I maxIncrease = new Vector4I(0,0,0,0);
		foreach (var e in _debuffs)
		{
			if (e.Item1._increase[0] > maxIncrease[0])
			{
				maxIncrease[0] = e.Item1._increase[0];
			}
			if (e.Item1._increase[1] > maxIncrease[1])
			{
				maxIncrease[1] = e.Item1._increase[1];
			}
			if (e.Item1._increase[2] > maxIncrease[2])
			{
				maxIncrease[2] = e.Item1._increase[2];
			}
			if (e.Item1._increase[3] > maxIncrease[3])
			{
				maxIncrease[3] = e.Item1._increase[3];
			}
		}
		_firstResource._increase = maxIncrease;
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
		if (maxIncrease == new Vector4I(0,0,0,0))
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
