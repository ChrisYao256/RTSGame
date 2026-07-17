using Godot;
using System;

namespace RTSGame.Units;

public partial class PermBurn : Effect
{
	PermBurnResource _firstResource;
	Timer _timer;

	public PermBurn(PermBurnResource resource) : base(resource)
	{
		_firstResource = resource;
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
		_timer.WaitTime = _firstResource._time;
		_timer.Timeout += () =>
		{
			RemoveEffectResource();
			RemoveEffectNode();
			_parentUnit.Modulate = new Color(1, 1, 1);
			_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		};
		_parentUnit.Modulate = ThemePalette.Green;
		_timer.Start();
	}

	public void AddResource(PermBurnResource newResource)
	{
		_timer.WaitTime = Math.Max(_timer.TimeLeft, newResource._time);
		_timer.Start();
	}

	protected override void UpdateTempDebuffIcon(UpgradeButton button)
	{
		if (_timer is null)
		{
			return;
		}
		button.UpdateAffordabilityDisplay((float)_timer.TimeLeft / _firstResource._time);
	}
}
