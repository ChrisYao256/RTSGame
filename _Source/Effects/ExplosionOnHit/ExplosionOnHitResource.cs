using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class ExplosionOnHitResource : EffectResource
{
	[Export] public float _explosionRadius;
	[Export] public int _explosionDamage;
	[Export] public PackedScene ExplosionVisualScene;

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = "Every hit causes an explosion, dealing " + _explosionDamage.ToString() + " damage to all nearby enemies in a " + _explosionRadius + " radius.";
	}

	public override Effect CreateNode()
	{
		return new ExplosionOnHit(this);
	}
}
