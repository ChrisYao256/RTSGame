using Godot;
using RTSGame.Units;

public partial class MultiHitRampUpDamage : Effect
{
	private MultiHitRampUpDamageResource _resource;

	private Unit _lastTarget;

	public MultiHitRampUpDamage(MultiHitRampUpDamageResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		_parentUnit = unit;
		unit.Connect(Unit.SignalName.BeforeHitEnemy, Callable.From<Unit>(OnBeforeHitEnemy));
		unit.Connect(Unit.SignalName.VolleyEnded, Callable.From(OnVolleyEnded));
	}

	protected override void OnBeforeHitEnemy(Unit target)
	{
		if (target == _lastTarget)
		{
			_parentUnit.IncreaseWeaponBuffPercent(_resource._increaseAmount);
		}
		_lastTarget = target;
	}

	protected override void OnVolleyEnded()
	{
		_parentUnit.SetWeaponBuffPercent(0);
		_lastTarget = null;
	}
}