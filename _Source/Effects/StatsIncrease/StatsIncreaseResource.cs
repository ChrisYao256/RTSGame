using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class StatsIncreaseResource : EffectResource
{
	[Export]
	public int _damageIncrease;

	[Export]
	public double _attackSpeedIncrease;

	[Export]
	public int _maxHpIncrease;

	[Export]
	public float _rangeIncrease;

	[Export]
	public float _speedIncrease;

	[Export]
	public double _attackDelayModifierIncrease;

	public override Effect CreateNode()
	{
		return new StatsIncrease(this);
	}
}
