using Godot;
using Godot.Collections;
using RTSGame.Source;
using RTSGame.Units;

public partial class AdjacencyTransform : Effect
{
	private AdjacencyTransformResource _resource;
	private Array<Vector2I> _usedTowers;

	public AdjacencyTransform(AdjacencyTransformResource resource) : base(resource)
	{
		_resource = resource;

	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.PlacedTower, Callable.From<TowerUnit>(OnPlacedTower));
		unit.Connect(Unit.SignalName.Creation, Callable.From(OnCreation));
	}

	protected override void OnCreation()
	{
		CheckTransform();
	}

	private void CheckTransform()
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I location = parentTower._gridLocation;
		foreach (Dictionary<Vector2I, Array<string>> dict in _resource._requirements)
		{
			bool met = true;
			_usedTowers = [];
			foreach (Vector2I relativePos in dict.Keys)
			{
				Vector2I position = relativePos + location;
				if (!parentTower._grid.IsCellVacant(position))
				{
					TowerUnit tower = parentTower._grid.GetTowerOnCell(position);
					if (!dict[relativePos].Contains(tower._internalName))
					{
						met = false;
					}
					else
					{
						_usedTowers.Add(position);
					}
				}
				else
				{
					met = false;
				}
			}
			if (met)
			{
				TransformTower();
				return;
			}
		}

	}

	protected void TransformTower()
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		if (_resource._removeUsedTowers)
		{
			foreach (Vector2I position in _usedTowers)
			{
				parentTower._tdManager._towerManager.RemoveTower(position);
			}
		}
		parentTower._tdManager._towerManager.TransformTower(parentTower._gridLocation, _resource._transformedUnit);
	}

	protected override void OnPlacedTower(TowerUnit tower)
	{
		CheckTransform();
	}
}
