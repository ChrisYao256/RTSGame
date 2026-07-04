using Godot;
using Godot.Collections;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class SpawnUnitOnDeathResource : EffectResource
{
	[Export]
	public Array<InvaderStatsIncreaseResource> _spawnedUnits;

	public float _jitter = 20f;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		SpawnUnitOnDeathResource typedOldResource = (SpawnUnitOnDeathResource)oldResource;

		for (int i = 0; i < _spawnedUnits.Count; i++)
		{
			_spawnedUnits[i].MergeWithOld(typedOldResource._spawnedUnits[i], []);
		}
		return false;
	}

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
