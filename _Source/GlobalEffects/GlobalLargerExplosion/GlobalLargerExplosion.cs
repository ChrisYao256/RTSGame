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
public partial class GlobalLargerExplosion : GlobalEffect
{
	GlobalLargerExplosionResource _resource;

	public GlobalLargerExplosion(GlobalLargerExplosionResource resource) : base(resource)
	{
		_resource = resource;
	}

	protected override void OnExplosion(ExplosionContext context)
	{
		context._radius *= (1f + _resource._extraRange);
	}
}
