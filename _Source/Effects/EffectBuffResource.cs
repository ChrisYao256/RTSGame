using Godot;
using Godot.Collections;
namespace RTSGame.Units;

public abstract partial class EffectBuffResource : EffectResource
{

	public virtual void ApplyBuff(Array<EffectResource> candidates)
	{

	}
}