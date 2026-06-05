using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class SlowAttack : Effect
{
	SlowAttackResource _firstResource;

	List<(SlowAttackResource, Timer)> _debuffs = [];

	public SlowAttack(SlowAttackResource resource) : base(resource)
	{
		_firstResource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		if (_parentUnit._weapon is null)
		{
			RemoveEffectResource();
			RemoveEffectNode();
			_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
			return;
		}
		OnCreation();
		AddResource(_firstResource);
	}

	public void AddResource(SlowAttackResource newResource)
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

	public void RecalculateDebuff()
	{
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

		_parentUnit.SetAttackSpeedDebuff(maxReduction);
		if (maxReduction == 0)
		{
			RemoveEffectResource();
			RemoveEffectNode();
			_parentUnit.Modulate = new Color(1, 1, 1);
			_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		}
		else
		{
			_parentUnit.Modulate = new Color(0.4f, 0.4f, 1.0f);
		}
	}
}
