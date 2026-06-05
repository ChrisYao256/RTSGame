
namespace RTSGame.Units;

public partial class DamageOverTimeBuff : Effect
{
	DamageOverTimeBuffResource _resource;

	public DamageOverTimeBuff(DamageOverTimeBuffResource resource) : base(resource)
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
		DamageOverTimeResource resource = (DamageOverTimeResource)_parentUnit.GetEffect(typeof(DamageOverTimeResource));
		resource._damage += _resource._damage;
		resource._time += _resource._time;
		resource.SetDescription();
	}
}
