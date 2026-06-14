using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class ImmediateTransformResource : EffectResource
{
	[Export]
	public string _transformedUnit;

	[Export]
	private string _description;

	public override void SetDescription()
	{
		_effectDescription = _description;
	}

	public override Effect CreateNode()
	{
		return new ImmediateTransform(this);
	}
}
