using Godot;
using Godot.Collections;
namespace RTSGame.Units;

[GlobalClass]
public partial class NearbyIncomeIncreaseResource : EffectResource
{
	[Export]
	public Vector4I _increaseAmount;

	[Export]
	public Array<Vector2I> _area = new Array<Vector2I>();

	public override void SetDescription()
	{
		_effectDescription = "Increase $ production of nearby Power Plants and Reactors by " + _increaseAmount;
	}

	public override Effect CreateNode()
	{
		return new NearbyIncomeIncrease(this);
	}
}
