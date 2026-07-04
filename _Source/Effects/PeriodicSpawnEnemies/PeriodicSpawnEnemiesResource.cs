using Godot;
using Godot.Collections;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class PeriodicSpawnEnemiesResource : EffectResource
{
	[Export]
	public Array<InvaderStatsIncreaseResource> _buffResource;

	[Export]
	public float _spawnInterval;

	public float _jitter = 20f;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		PeriodicSpawnEnemiesResource typedOldResource = (PeriodicSpawnEnemiesResource)oldResource;

		for (int i = 0; i < _buffResource.Count; i++)
		{
			_buffResource[i].MergeWithOld(typedOldResource._buffResource[i], []);
		}
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = "Summons more enemies periodically.";
	}

	public override Effect CreateNode()
	{
		return new PeriodicSpawnEnemies(this);
	}
}
