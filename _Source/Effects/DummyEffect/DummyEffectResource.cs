using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class DummyEffectResource : EffectResource
{

	public override void SetDescription()
	{
	}

	public override Effect CreateNode()
	{
		return new DummyEffect(this);
	}
}
