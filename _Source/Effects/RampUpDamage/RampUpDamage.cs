using Godot;
using RTSGame.Units;

public partial class RampUpDamage : Effect
{
	private Timer _timer;

	private RampUpDamageResource _resource;

	private int _increaseCount = 0;

	public RampUpDamage(RampUpDamageResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		_parentUnit = unit;
		unit.Connect(Unit.SignalName.BeginAttack, Callable.From<Unit>(OnBeginAttack));
		unit.Connect(Unit.SignalName.StopAttack, Callable.From<Unit>(OnStopAttack));
	}

	protected override void OnBeginAttack(Unit target)
	{
		_timer?.QueueFree();
		_timer = new Timer();
		AddChild(_timer);
		_timer.WaitTime = _resource._increaseInterval;
		_timer.OneShot = _resource._oneTime;
		_timer.Timeout += () =>
		{
			_parentUnit.IncreaseWeaponModifier(_resource._increaseAmount);
			_increaseCount++;
		};
		_timer.Start();
	}

	protected override void OnStopAttack(Unit target)
	{
		_parentUnit.IncreaseWeaponModifier(-_resource._increaseAmount * _increaseCount);
		_timer?.Stop();
	}
}