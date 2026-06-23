using Godot;
using Godot.Collections;
namespace RTSGame.Units;

[GlobalClass]
public partial class SpawnUnitOnDeathResource : EffectResource
{
	[Export]
	public Array<string> _spawnedUnit;

	[Export]
	public InvaderStatsIncreaseResource _buffResource;

	public float _jitter = 20f;

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = "Spawns units when dying.";
	}

	public override Effect CreateNode()
	{
		return new SpawnUnitOnDeath(this);
	}
}
