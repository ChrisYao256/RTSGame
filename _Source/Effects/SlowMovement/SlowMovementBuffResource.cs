using Godot;
using Godot.Collections;
using System.Linq;
namespace RTSGame.Units;

[GlobalClass]
public partial class SlowMovementBuffResource : EffectBuffResource
{
	[Export] public float _percentDecrease;
	[Export] public float _time;

	public override void SetDescription()
	{
		_effectDescription = "";
	}

	public override void ApplyBuff(Array<EffectResource> candidates)
	{
		SlowMovementResource resource = candidates.OfType<SlowMovementResource>().FirstOrDefault();
		resource._percentDecrease += _percentDecrease;
		resource._time += _time;
		resource.SetDescription();
	}

	public override Effect CreateNode()
	{
		return new SlowMovementBuff(this);
	}
}
