using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class DamageOverTime : Effect
{
	DamageOverTimeResource _firstResource;

	List<(DamageOverTimeResource, Timer)> _debuffs = [];

	const int MaxDamage = 1000;

	int _totalDamage;
	double _queuedDamage = 0;

	public DamageOverTime(DamageOverTimeResource resource) : base(resource)
	{
		_firstResource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
		AddResource(_firstResource);
	}

	public void AddResource(DamageOverTimeResource newResource)
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
		_totalDamage = 0;
		foreach (var e in _debuffs)
		{
			_totalDamage += e.Item1._damage;
		}
		int firstDamage = _firstResource._damage;
		_firstResource._damage = _totalDamage;
		_firstResource.SetDescription();
		_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		_firstResource._damage = firstDamage;

		_totalDamage = Math.Min(MaxDamage, _totalDamage);

		if (_totalDamage == 0)
		{
			RemoveEffectResource();
			RemoveEffectNode();
			_parentUnit.Modulate = new Color(1, 1, 1);
			_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		}
		else
		{
			_parentUnit.Modulate = new Color(1f, 0.6f, 0.2f);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_queuedDamage += delta * _totalDamage;
		if (_queuedDamage > 1)
		{
			_parentUnit.Hit((int)Mathf.FloorToInt(_queuedDamage), null, true);
			_queuedDamage -= Mathf.FloorToInt(_queuedDamage);
		}
	}
}
