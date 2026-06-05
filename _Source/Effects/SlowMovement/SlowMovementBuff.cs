
namespace RTSGame.Units;

public partial class SlowMovementBuff : Effect
{
	SlowMovementBuffResource _resource;

	public SlowMovementBuff(SlowMovementBuffResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
	}

	protected override void OnCreation()
	{
		SlowMovementResource resource = (SlowMovementResource)_parentUnit.GetEffect(typeof(SlowMovementResource));
		resource._percentDecrease += _resource._percentDecrease;
		resource._time += _resource._time;
		resource.SetDescription();
	}
}
