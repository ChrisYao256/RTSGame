using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class SpeedUpOnHit : Effect
{
	SpeedUpOnHitResource _resource;
	private float _speedIncrease; 

	public SpeedUpOnHit(SpeedUpOnHitResource resource) : base(resource)
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
		if (_speedIncrease >= _resource._speedIncreaseCap || unit is null)
		{
			return;
		}
		_speedIncrease += _resource._speedIncreasePerHit;
		_parentUnit.IncreaseSpeedModifier(_resource._speedIncreasePerHit);
	}
}
