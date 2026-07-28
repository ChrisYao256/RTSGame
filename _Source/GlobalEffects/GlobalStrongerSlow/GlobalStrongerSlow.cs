using Godot;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

/// <summary>
/// Should be obsolete, since every unit has a StatsIncrease _data in its _effects.
/// </summary>
public partial class GlobalStrongerSlow : GlobalEffect
{
	GlobalStrongerSlowResource _resource;

	public GlobalStrongerSlow(GlobalStrongerSlowResource resource) : base(resource)
	{
		_resource = resource;
	}

	protected override void OnAddNewEffect(AddEffectContext addEffectContext)
	{
		if (addEffectContext._effect.GetType() == typeof(SlowMovementResource))
		{
			SlowMovementResource slow = (SlowMovementResource)addEffectContext._effect;
			slow._percentDecrease += _resource._extraSlow;
		}
	}
}
