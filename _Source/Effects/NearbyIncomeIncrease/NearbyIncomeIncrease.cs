using RTSGame.Units;
using System;
using System.Collections;
using Godot;
using Godot.Collections;
using System.Linq;

public partial class NearbyIncomeIncrease : Effect
{
	NearbyIncomeIncreaseResource _resource;

	public NearbyIncomeIncrease(NearbyIncomeIncreaseResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		unit.Connect(Unit.SignalName.PlacedTower, Callable.From<TowerUnit>(OnPlacedTower));
		OnCreation();
	}

	protected override void OnCreation()
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I location = parentTower._gridLocation;
		foreach (Vector2I relativePos in _resource._area)
		{
			Vector2I position = relativePos + location;
			if (!parentTower._grid.IsCellVacant(position))
			{
				TowerUnit tower = parentTower._grid.GetTowerOnCell(position);
				if (tower._internalName == "Mine" || tower._internalName == "Reactor")
				{
					MoneyOnWaveStartResource incomeResource = (tower._effects).OfType<MoneyOnWaveStartResource>().FirstOrDefault();
					incomeResource._money += _resource._increaseAmount;
					incomeResource.SetDescription();
				}
			}
		}
	}

	protected override void OnPlacedTower(TowerUnit tower)
	{
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I delta = tower._gridLocation - parentTower._gridLocation;
		if (_resource._area.Contains(delta))
		{
			if (tower._name == "Mine" || tower._name == "Reactor")
			{
				MoneyOnWaveStartResource incomeResource = (tower._effects).OfType<MoneyOnWaveStartResource>().FirstOrDefault();
				incomeResource._money += _resource._increaseAmount;
				incomeResource.SetDescription();
			}
		}
	}

	public override void RemoveEffectNode()
	{
		if (!GodotObject.IsInstanceValid(_parentUnit))
		{
			QueueFree();
			return;
		}
		TowerUnit parentTower = (TowerUnit)_parentUnit;
		Vector2I location = parentTower._gridLocation;
		foreach (Vector2I relativePos in _resource._area)
		{
			Vector2I position = relativePos + location;
			if (!parentTower._grid.IsCellVacant(position))
			{
				TowerUnit tower = parentTower._grid.GetTowerOnCell(position);
				if (tower._name == "Mine" || tower._name == "Reactor")
				{
					MoneyOnWaveStartResource incomeResource = (tower._effects).OfType<MoneyOnWaveStartResource>().FirstOrDefault();
					incomeResource._money -= _resource._increaseAmount;
					incomeResource.SetDescription();
				}
			}
		}
		_parentUnit._effects.Remove(_resource);
		QueueFree();
	}
}
