using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class ExplosionOnHitResource : EffectResource
{
	[Export] public float _explosionRadius;
	[Export] public int _explosionDamage;
	[Export] public PackedScene ExplosionVisualScene;
	[Export] public int _explosiveHitInterval = 1;

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = $"Every {_explosiveHitInterval} hit causes an explosion dealing {_explosionDamage} damage.";
	}

	public override Effect CreateNode()
	{
		return new ExplosionOnHit(this);
	}
}
