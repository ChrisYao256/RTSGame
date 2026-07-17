using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class ExplosionOnKillResource : EffectResource
{
	[Export] public float _explosionRadius;
	[Export] public int _explosionDamage;
	[Export] public PackedScene ExplosionVisualScene;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		ExplosionOnKillResource typedOldResource = (ExplosionOnKillResource)oldResource;

		typedOldResource._explosionRadius += _explosionRadius;
		typedOldResource._explosionDamage += _explosionDamage;
		typedOldResource.SetDescription();

		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = $"Every kill causes an explosion dealing {_explosionDamage} damage.";
	}

	public override void SetUpgradeDescription()
	{
		_effectDescription = "";
		if (_explosionDamage > 0)
		{
			_effectDescription += $"Explosion damage +{_explosionDamage}. ";
		}

		if (_explosionRadius > 0)
		{
			_effectDescription += "Increases explosion size.";
		}
	}

	public override Effect CreateNode()
	{
		return new ExplosionOnKill(this);
	}
}
