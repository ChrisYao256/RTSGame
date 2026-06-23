using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class Revive : Effect
{
	ReviveResource _resource;

	public Revive(ReviveResource resource) : base(resource)
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
		if (_parentUnit._hp < 0)
		{
			if (_resource._reviveCount > 0)
			{
				_parentUnit.IncreaseHp(-_parentUnit._hp + _parentUnit.GetHpMax(), true);
				_resource._reviveCount--;
				_resource.SetDescription();
				MakeFloatingAnimation();
			}
		}
	}
}
