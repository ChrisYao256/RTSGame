using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class ExplosionOnHitBuffResource : EffectResource
{
	[Export] public float _explosionRadius;
	[Export] public int _explosionDamage;

	public override void SetDescription()
	{
		if (_explosionDamage != 0)
		{
			_effectDescription = "Increases explosion damage by " + _explosionDamage + "\n";
		}
		if (_explosionRadius != 0)
		{
			_effectDescription = "Increases explosion radius by " + _explosionRadius + "\n";
		}
	}

	public override Effect CreateNode()
	{
		return new ExplosionOnHitBuff(this);
	}
}
