using Godot;
using RTSGame.Source;
namespace RTSGame.Units;

public partial class MakeGroundWalkable : Effect
{
	MakeGroundWalkableResource _resource;

	public MakeGroundWalkable(MakeGroundWalkableResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.Movement, Callable.From<Vector2I, Vector2I>(OnMovement));
		OnCreation();
	}

	protected override void OnCreation()
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		parentTower._grid.MakeTileWalkable(parentTower._gridLocation, bidirectional: _resource._bidirectional);
	}

	protected override void OnMovement(Vector2I origin, Vector2I target)
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		parentTower._grid.MakeTileWalkable(origin, false);
		parentTower._grid.MakeTileWalkable(target, true, bidirectional: _resource._bidirectional);
	}

	public override void RemoveEffectNode()
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		parentTower._grid.MakeTileWalkable(parentTower._gridLocation, false);
		_parentUnit._effects.Remove(_resource);
		QueueFree();
	}
}
