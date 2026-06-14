using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class DummyEffectResource : EffectResource
{

	[Export]
	private string _description;

	public override void SetDescription()
	{
		_effectDescription = _description;
	}

	public override Effect CreateNode()
	{
		return new DummyEffect(this);
	}
}
