using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class ExplosionOnHitResource : EffectResource
{
	[Export] public float _explosionRadius;
	[Export] public int _explosionDamage;
	[Export] public PackedScene ExplosionVisualScene;

	public override Effect CreateNode()
	{
		return new ExplosionOnHit(this);
	}
}
