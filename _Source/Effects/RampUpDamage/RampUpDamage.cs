using Godot;
using RTSGame.Units;

public partial class RampUpDamage : Effect
{
	private Timer _timer;

	private RampUpDamageResource _resource;

	private int _oldModifier;

	//public SelfHealing(int healAmount, double healInterval)
	//{
	//	_healAmount = healAmount;
	//	_healInterval = healInterval;
	//}

	public RampUpDamage(RampUpDamageResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		_parentUnit = unit;
		unit.Connect(Unit.SignalName.BeginAttack, Callable.From<Unit>(OnBeginAttack));
		unit.Connect(Unit.SignalName.StopAttack, Callable.From<Unit>(OnStopAttack));
		_oldModifier = _parentUnit._weapon._damageModifier;
	}

	protected override void OnBeginAttack(Unit target)
	{
		_oldModifier = _parentUnit._weapon._damageModifier;
		
		_timer?.QueueFree();
		_timer = new Timer();
		AddChild(_timer);
		_timer.WaitTime = _resource._increaseInterval;
		_timer.OneShot = _resource._oneTime;
		_timer.Timeout += () => _parentUnit.IncreaseWeaponModifier(_resource._increaseAmount);
		_timer.Start();
	}

	protected override void OnStopAttack(Unit target)
	{
		_parentUnit.SetWeaponModifier(_oldModifier);
		_timer?.Stop();
	}
}