using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class TeslaTakeMoreDamageResource : EffectResource
{
	[Export]
	public float _damageIncrease;

	[Export]
	public float _duration;

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Small;
		_effectDescription = $"Takes {_damageIncrease * 100}% more damage from tesla turrets.";
	}

	public override Effect CreateNode()
	{
		return new TeslaTakeMoreDamage(this);
	}
}
