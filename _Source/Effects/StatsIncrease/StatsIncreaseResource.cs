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

	public override void SetDescription()
	{
		_effectDescription = "";
		if (_damageIncrease != 0)
		{
			_effectDescription += "Increase damage by " + _damageIncrease + "\n";
		}

		if (_rangeIncrease != 0)
		{

			_effectDescription += "Increase range by " + _rangeIncrease + "\n";
		}

		if (_speedIncrease != 0)
		{

			_effectDescription += "Increase speed by " + _speedIncrease + "\n";
		}

		if (_attackSpeedIncrease != 0)
		{

			_effectDescription += "Increase attack speed by " + _attackSpeedIncrease + "\n";
		}

		if (_attackDelayModifierIncrease != 0)
		{
			_effectDescription += "Increase attack delay by " + _attackDelayModifierIncrease + "\n";
		}
	}

	public override Effect CreateNode()
	{
		return new StatsIncrease(this);
	}
}
