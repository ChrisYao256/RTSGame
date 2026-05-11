using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class RampUpDamageResource : EffectResource
{
	[Export]
	public int _increaseAmount;

	[Export]
	public double _increaseInterval;

	[Export]
	public bool _oneTime = false;

	public override Effect CreateNode()
	{
		return new RampUpDamage(this);
	}
}
