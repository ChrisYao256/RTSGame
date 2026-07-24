using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class OneTimeShieldResource : EffectResource
{
	[Export] public int _stack = 1;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		OneTimeShieldResource typedOldResource = (OneTimeShieldResource)oldResource;

		typedOldResource._stack += _stack;
		typedOldResource.SetDescription();

		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = $"Ignore the first {_stack} instances of damage.";
	}

	public override Effect CreateNode()
	{
		return new OneTimeShield(this);
	}
}
