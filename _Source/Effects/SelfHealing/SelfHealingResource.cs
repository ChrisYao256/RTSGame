using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class SelfHealingResource : EffectResource
{
	[Export]
	public int _healAmount;

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = "Regenerates " + _healAmount + " Hp every second.";
	}

	public override Effect CreateNode()
	{
		return new SelfHealing(this);
	}
}
