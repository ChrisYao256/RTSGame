using Godot;

namespace RTSGame.Units;
public partial class RampUpDamage : Effect
{
	private Timer _timer;

	[Export]
	private int _increaseAmount;

	[Export]
	private double _increaseInterval;

	//public SelfHealing(int healAmount, double healInterval)
	//{
	//	_healAmount = healAmount;
	//	_healInterval = healInterval;
	//}

	public override void ConnectSignals(Unit unit)
	{
		_parentUnit = unit;
		unit.Connect(Unit.SignalName.BeginAttack, Callable.From<Unit>(OnBeginAttack));
	}

	protected override void OnBeginAttack(Unit target)
	{
		_parentUnit.SetWeaponModifier(0);
		_timer?.QueueFree();
		_timer = new Timer();
		AddChild(_timer);
		_timer.WaitTime = _increaseInterval;
		_timer.OneShot = false;
		_timer.Timeout += () => _parentUnit.SetWeaponModifier(_increaseAmount);
		_timer.Start();
	}
}