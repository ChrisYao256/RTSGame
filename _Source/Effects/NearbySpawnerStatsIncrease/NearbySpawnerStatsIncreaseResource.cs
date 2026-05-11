using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class NearbySpawnerStatsIncreaseResource : EffectResource
{
	[Export]
	public SpawnerStatsIncreaseResource _buffResource;

	[Export]
	public Array<Vector2I> _area = new Array<Vector2I>();

	public override Effect CreateNode()
	{
		return new NearbySpawnerStatsIncrease(this);
	}
}
