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

	public override void SetDescription()
	{
		_effectDescription = "Increases tower damage by " + _increaseAmount + " for every " + _increaseInterval + " seconds the tower has spent attacking the same enemy.";
	}

	public override Effect CreateNode()
	{
		return new RampUpDamage(this);
	}
}
