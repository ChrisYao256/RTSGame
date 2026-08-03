using Godot;
using Godot.Collections;
namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalMultiEffectResource : GlobalEffectResource
{
	[Export]
	public Array<GlobalEffectResource> _effects;


	public override void SetDescription()
	{
		foreach (var effect in _effects)
		{
			effect.SetDescription();
			_effectDescription += effect._effectDescription;
		}
	}

	public override GlobalEffect CreateNode()
	{
		return new GlobalMultiEffect(this);
	}
}
