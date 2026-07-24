using Godot;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class MoneyOnWaveStartOneType : Effect
{
	MoneyOnWaveStartOneTypeResource _resource;

	public MoneyOnWaveStartOneType(MoneyOnWaveStartOneTypeResource resource) : base(resource)
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
		if (_resource._giveLowest)
		{
			Vector4I v = parentTower._tdManager._money;
			
			int minVal = Mathf.Min(Mathf.Min(v.X, v.Y), Mathf.Min(v.Z, v.W));

			Vector4I w = new Vector4I(0, 0, 0, _resource._money);
			if (v.X == minVal) w = new Vector4I(_resource._money, 0, 0, 0);
			if (v.Y == minVal) w = new Vector4I(0, _resource._money, 0, 0);
			if (v.Z == minVal) w = new Vector4I(0, 0, _resource._money, 0);
			
			parentTower._tdManager.GainMoney(w);
		}
		else
		{
			int randomIndex = (int)(GD.Randi() % 4);

			parentTower._tdManager.GainMoney(new Vector4I(
					randomIndex == 0 ? _resource._money : 0,
					randomIndex == 1 ? _resource._money : 0,
					randomIndex == 2 ? _resource._money : 0,
					randomIndex == 3 ? _resource._money : 0
			)); 
		}
	}
}
