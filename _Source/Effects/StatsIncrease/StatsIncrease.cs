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

	protected override void OnCreation()
	{
		_parentUnit.IncreaseWeaponModifier(_resource._damageIncrease);
		_parentUnit.IncreaseWeaponRangeModifier(_resource._rangeIncrease);
		_parentUnit.IncreaseAttackSpeedModifier(_resource._attackSpeedIncrease);
		_parentUnit.IncreaseSpeedModifier(_resource._speedIncrease);
		_parentUnit.IncreaseAttackDelayModifier(_resource._attackDelayModifierIncrease);
	}
}
