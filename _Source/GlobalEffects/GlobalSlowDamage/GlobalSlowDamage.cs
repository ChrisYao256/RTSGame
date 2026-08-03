using Godot;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class GlobalSlowDamage: GlobalEffect
{
	GlobalSlowDamageResource _resource;

	public GlobalSlowDamage(GlobalSlowDamageResource resource) : base(resource)
	{
		_resource = resource;
	}

	protected override void OnAddNewEffect(AddEffectContext addEffectContext)
	{
		if (IsInstanceValid(addEffectContext._target))
		if (addEffectContext._effect is SlowMovementResource slow)
		{
			if (slow._percentDecrease > 0)
			{
				DamageOverTimeResource damageOverTime = new();
				damageOverTime._percent = _resource._percentDamage;
				damageOverTime._time = slow._time;
				addEffectContext._target.AddEffect(damageOverTime);
			}
			else if (slow._percentDecrease < 0)
			{
				DamageOverTimeResource damageOverTime = (DamageOverTimeResource)addEffectContext._target._effects.First(e => e.GetType() == typeof(DamageOverTimeResource));
				damageOverTime._effect.RemoveEffectNode();
				damageOverTime._effect.RemoveEffectResource();
			}
		}
	}
}
