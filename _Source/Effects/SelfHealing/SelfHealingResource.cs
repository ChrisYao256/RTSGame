using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class SelfHealingResource : EffectResource
{
	[Export]
	public int _healAmount;

	[Export]
	public double _healInterval;

	public override Effect CreateNode()
	{
		return new SelfHealing(this);
	}
}
