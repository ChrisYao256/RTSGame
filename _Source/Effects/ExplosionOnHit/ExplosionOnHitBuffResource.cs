using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class ExplosionOnHitBuffResource : EffectResource
{
	[Export] public float _explosionRadius;
	[Export] public int _explosionDamage;

	public override Effect CreateNode()
	{
		return new ExplosionOnHitBuff(this);
	}
}
