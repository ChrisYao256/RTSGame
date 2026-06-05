using Godot;
using Godot.Collections;
namespace RTSGame.Units;

[GlobalClass]
public partial class DebuffOnHitBuffResource : EffectResource
{
	[Export] public Array<EffectResource> _debuffs;

	public override void SetDescription()
	{
		_effectDescription = "";

	}

	public override Effect CreateNode()
	{
		return new DebuffOnHitBuff(this);
	}
}
