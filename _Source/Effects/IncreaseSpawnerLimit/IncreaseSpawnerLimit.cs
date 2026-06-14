using Godot;
namespace RTSGame.Units;

public partial class IncreaseSpawnerLimit : Effect
{
	IncreaseSpawnerLimitResource _resource;

	public IncreaseSpawnerLimit(IncreaseSpawnerLimitResource resource) : base(resource)
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
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		parentTower._tdManager.IncreaseSpawnerLimit(1);
	}

	public override void RemoveEffectNode()
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		parentTower._tdManager.IncreaseSpawnerLimit(-1);
		_parentUnit._effects.Remove(_resource);
		QueueFree();
	}
}
