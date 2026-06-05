using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class LowerArmor : Effect
{
	LowerArmorResource _firstResource;

	List<(LowerArmorResource, Timer)> _debuffs = [];

	public LowerArmor(LowerArmorResource resource) : base(resource)
	{
		_firstResource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
		AddResource(_firstResource);
	}

	public void AddResource(LowerArmorResource newResource)
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
		int maxReduction = 0;
		foreach (var e in _debuffs)
		{
			if (e.Item1._change < maxReduction)
			{
				maxReduction = e.Item1._change;
			}	
		}
		_firstResource._change = maxReduction;
		_firstResource.SetDescription();
		_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);

		_parentUnit.SetArmorDebuff(maxReduction);
		if (maxReduction == 0)
		{
			RemoveEffectResource();
			RemoveEffectNode();
			_parentUnit.Modulate = new Color(1, 1, 1);
			_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		}
		else
		{
			_parentUnit.Modulate = new Color (1, 0.2f, 0);
		}
	}
}
