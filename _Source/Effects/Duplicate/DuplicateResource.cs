using Godot;
using Godot.Collections;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class DuplicateResource : EffectResource
{
	[Export]
	public float _delay;

	public float _jitter = 20f;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = $"Spawns an copy of itself after {_delay}::duration::";
	}

	public override Effect CreateNode()
	{
		return new Duplicate(this);
	}
}
