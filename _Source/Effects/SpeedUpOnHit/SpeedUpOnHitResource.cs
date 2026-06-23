using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class SpeedUpOnHitResource : EffectResource
{
	[Export]
	public float _speedIncreasePerHit;

	[Export]
	public float _speedIncreaseCap;
	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = "Speeds up by " + _speedIncreasePerHit + " when hit. Max:"+ _speedIncreaseCap;
	}

	public override Effect CreateNode()
	{
		return new SpeedUpOnHit(this);
	}
}
