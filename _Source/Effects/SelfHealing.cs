using Godot;

namespace RTSGame.Units;
public partial class SelfHealing : Effect
{
	private Timer _timer;

	[Export]
	private int _healAmount;

	[Export]
	private double _healInterval;

	//public SelfHealing(int healAmount, double healInterval)
	//{
	//	_healAmount = healAmount;
	//	_healInterval = healInterval;
	//}

	public override void _Ready()
	{
		_timer = new Timer();
		AddChild(_timer);
		_timer.WaitTime = _healInterval;
		_timer.OneShot = false;
		_timer.Timeout += () => _parentUnit.IncreaseHp(_healAmount);
		_timer.Start();
	}
}