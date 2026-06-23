using Godot;
using Godot.Collections;
namespace RTSGame.Units;

[GlobalClass]
public partial class ApplyEffectToTowerBuffResource : EffectResource
{
	[Export] public Array<EffectResource> _debuffs;

	public override void SetDescription()
	{
		return;

	}

	public override Effect CreateNode()
	{
		return new ApplyEffectToTowerBuff(this);
	}
}
