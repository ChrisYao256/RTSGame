using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class AdjacencyTransformResource : EffectResource
{
	[Export]
	public string _transformedUnit;

	[Export]
	public Array<Dictionary<Vector2I, Array<string>>> _requirements;

	[Export]
	public bool _removeUsedTowers = false;

	public override void SetDescription()
	{
		
	}

	public override Effect CreateNode()
	{
		return new AdjacencyTransform(this);
	}
}
