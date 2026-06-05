using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class NearbyTowerStatsIncreaseResource : EffectResource
{
	[Export]
	public StatsIncreaseResource _buffResource;

	[Export]
	public Array<Vector2I> _area = new Array<Vector2I>();


	public override void SetDescription()
	{
		_effectDescription = "Buffs nearby spawners with the following: \n";
		_buffResource.SetDescription();
		_effectDescription += _buffResource._effectDescription;
	}

	public override Effect CreateNode()
	{
		return new NearbyTowerStatsIncrease(this);
	}
}
