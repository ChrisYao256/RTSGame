using Godot;
using Godot.Collections;
using RTSGame.Source;
using RTSGame.Units;

public partial class ImmediateTransform : Effect
{
	private ImmediateTransformResource _resource;

	public ImmediateTransform(ImmediateTransformResource resource) : base(resource)
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
		TransformTower();
	}

	protected void TransformTower()
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		parentTower._tdManager._towerManager.TransformTower(parentTower._gridLocation, _resource._transformedUnit);
	}
}
