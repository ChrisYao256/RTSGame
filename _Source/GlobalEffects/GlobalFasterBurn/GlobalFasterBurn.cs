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
public partial class GlobalFasterBurn : GlobalEffect
{
	GlobalFasterBurnResource _resource;

	public GlobalFasterBurn(GlobalFasterBurnResource resource) : base(resource)
	{
		_resource = resource;
	}

	protected override void OnAddNewEffect(AddEffectContext addEffectContext)
	{
		if (addEffectContext._effect.GetType() == typeof(BurnResource))
		{
			BurnResource burn = (BurnResource)addEffectContext._effect;
			burn._decayRate -= _resource._percentIncrease;
		}
	}
}
