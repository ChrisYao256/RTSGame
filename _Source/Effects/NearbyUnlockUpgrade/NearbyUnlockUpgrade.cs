using RTSGame.Units;
using System;
using System.Collections.Generic;
using Godot;
using RTSGame.Source;

public partial class NearbyUnlockUpgrade : Effect
{
	private NearbyUnlockUpgradeResource _resource;

	public NearbyUnlockUpgrade(NearbyUnlockUpgradeResource resource) : base(resource)
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
				if (_resource._unlockSecondUpgrades)
				{
					tower.UnlockSecondUpgrades();
				}
				if (_resource._unlockThirdUpgrades)
				{
					tower.UnlockThirdUpgrade();
					tower.UnlockFourthUpgrade();
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
			if (_resource._unlockSecondUpgrades)
			{
				tower.UnlockSecondUpgrades();
			}
			if (_resource._unlockThirdUpgrades)
			{
				tower.UnlockThirdUpgrade();
				tower.UnlockFourthUpgrade();
			}
		}
	}
}
