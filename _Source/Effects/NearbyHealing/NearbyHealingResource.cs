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

	[Export]
	public bool _isShield;

	public override void SetDescription()
	{
		_effectDescription = $"{(_isShield? "Shields" : "Heals")} all units in a " + _radius + " radius for " + _healAmount + " every " + _healInterval + " seconds";
	}

	public override Effect CreateNode()
	{
		return new NearbyHealing(this);
	}
}
