
namespace RTSGame.Units;

public partial class DropMoreMoneyBuff : Effect
{
	DropMoreMoneyBuffResource _resource;

	public DropMoreMoneyBuff(DropMoreMoneyBuffResource resource) : base(resource)
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
		DropMoreMoneyResource resource = (DropMoreMoneyResource)_parentUnit.GetEffect(typeof(DropMoreMoneyResource));
		resource._increase += _resource._increase;
		resource._time += _resource._time;
		resource.SetDescription();
	}
}
