using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

/// <summary>
/// Should be obsolete, since every unit has a StatsIncrease _data in its _effects.
/// </summary>
public partial class StatsIncrease : Effect
{
	StatsIncreaseResource _resource;

	public StatsIncrease(StatsIncreaseResource resource) : base(resource)
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
		((TowerUnit)_parentUnit).AddTowerStatsIncrease(_resource);
		QueueFree();
	}
}
