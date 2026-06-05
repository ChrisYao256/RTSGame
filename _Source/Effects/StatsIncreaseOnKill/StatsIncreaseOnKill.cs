using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class StatsIncreaseOnKill : Effect
{
	StatsIncreaseOnKillResource _resource;
	int _stacks;

	public StatsIncreaseOnKill(StatsIncreaseOnKillResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.KilledUnit, Callable.From<Unit>(OnKilledEnemy));
		OnCreation();
	}

	protected override void OnKilledEnemy(Unit unit)
	{
		if (unit._teamId != _parentUnit._teamId)
		{
			if (_stacks < _resource._cap || _resource._cap == -1)
			{
				_parentUnit.AddEffect(_resource._buffResource);
				_stacks++;
				_resource._stack++;
				_resource.SetDescription();
				_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
			}
		}
	}
}
