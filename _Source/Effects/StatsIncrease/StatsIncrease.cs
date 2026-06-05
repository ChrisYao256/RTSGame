using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

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
		_parentUnit.IncreaseWeaponModifier(_resource._damageIncrease);
		_parentUnit.IncreaseWeaponPercentModifier(_resource._damagePercentIncrease);
		_parentUnit.IncreaseWeaponRangeModifier(_resource._rangeIncrease);
		_parentUnit.IncreaseAttackSpeedModifier(_resource._attackSpeedIncrease);
		_parentUnit.IncreaseSpeedModifier(_resource._speedIncrease);
		_parentUnit.IncreaseAttackDelayModifier(_resource._attackDelayModifierIncrease);
		_parentUnit.IncreasePierceCountModifier(_resource._pierceCount);
		_parentUnit.UpdateWeaponAttackZone(_resource._newZone);
		if (_resource._disableDelay)
		{
			_parentUnit.DisableAttackDelay();
		}
	}

	public void UpdateResource(StatsIncreaseResource newResource)
	{
		_parentUnit._effects.Remove(_resource);

		if (!GodotObject.IsInstanceValid(this))
		{
			return;
		}
		if (!GodotObject.IsInstanceValid(_parentUnit))
		{
			QueueFree();
			return;
		}
		_parentUnit.IncreaseWeaponModifier(-_resource._damageIncrease);
		_parentUnit.IncreaseWeaponPercentModifier(-_resource._damagePercentIncrease);
		_parentUnit.IncreaseWeaponRangeModifier(-_resource._rangeIncrease);
		_parentUnit.IncreaseAttackSpeedModifier(-_resource._attackSpeedIncrease);
		_parentUnit.IncreaseSpeedModifier(-_resource._speedIncrease);
		_parentUnit.IncreaseAttackDelayModifier(-_resource._attackDelayModifierIncrease);
		_parentUnit.IncreasePierceCountModifier(-_resource._pierceCount);

		_resource = newResource;
		_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);

		_parentUnit.IncreaseWeaponModifier(newResource._damageIncrease);
		_parentUnit.IncreaseWeaponPercentModifier(newResource._damagePercentIncrease);
		_parentUnit.IncreaseWeaponRangeModifier(newResource._rangeIncrease);
		_parentUnit.IncreaseAttackSpeedModifier(newResource._attackSpeedIncrease);
		_parentUnit.IncreaseSpeedModifier(newResource._speedIncrease);
		_parentUnit.IncreaseAttackDelayModifier(newResource._attackDelayModifierIncrease);
		_parentUnit.IncreasePierceCountModifier(newResource._pierceCount);
		if (newResource._disableDelay)
		{
			_parentUnit.DisableAttackDelay();
		}
		_parentUnit._effects.Add(_resource);
	}

	public override void RemoveEffectNode()
	{
		if (!GodotObject.IsInstanceValid(this))
		{
			return;
		}
		if (!GodotObject.IsInstanceValid(_parentUnit))
		{
			QueueFree();
			return;
		}
		_parentUnit.IncreaseWeaponModifier(-_resource._damageIncrease);
		_parentUnit.IncreaseWeaponPercentModifier(-_resource._damagePercentIncrease);
		_parentUnit.IncreaseWeaponRangeModifier(-_resource._rangeIncrease);
		_parentUnit.IncreaseAttackSpeedModifier(-_resource._attackSpeedIncrease);
		_parentUnit.IncreaseSpeedModifier(-_resource._speedIncrease);
		_parentUnit.IncreaseAttackDelayModifier(-_resource._attackDelayModifierIncrease);
		_parentUnit.IncreasePierceCountModifier(-_resource._pierceCount);
		_parentUnit._effects.Remove(_resource);
		QueueFree();
	}
}
