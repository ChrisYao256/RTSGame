using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class NearbyHealingResource : EffectResource
{
	[Export]
	public int _healAmount;

	[Export]
	public double _healInterval;

	[Export]
	public float _radius;

	[Export]
	public PackedScene _healingVisualScene;

	public override Effect CreateNode()
	{
		return new NearbyHealing(this);
	}
}
