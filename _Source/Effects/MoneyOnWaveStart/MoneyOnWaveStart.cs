using Godot;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class MoneyOnWaveStart: Effect
{
	MoneyOnWaveStartResource _resource;

	public MoneyOnWaveStart(MoneyOnWaveStartResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		parentTower._tdManager.Connect(TDManager.SignalName.NewWave, Callable.From(OnNewWave));
	}

	protected override void OnNewWave()
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		parentTower._tdManager.GainMoney(_resource._money);
	}
}
