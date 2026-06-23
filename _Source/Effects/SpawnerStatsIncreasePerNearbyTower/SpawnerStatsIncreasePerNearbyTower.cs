//using Godot;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace RTSGame.Units;

//public partial class SpawnerStatsIncreasePerNearbyTower : Effect
//{
//	SpawnerStatsIncreasePerNearbyTowerResource _resource;
//	InvaderStatsIncrease _createdEffect;
//	int _matches;

//	public SpawnerStatsIncreasePerNearbyTower(SpawnerStatsIncreasePerNearbyTowerResource resource) : base(resource)
//	{
//		_resource = resource;
//	}

//	public override void ConnectSignals(Unit unit)
//	{
//		base.ConnectSignals(unit);
//		unit.Connect(Unit.SignalName.PlacedTower, Callable.From<TowerUnit>(OnPlacedTower));
//		OnCreation();
//	}

//	protected override void OnCreation()
//	{
//		TowerUnit parentTower = (TowerUnit)_parentUnit;
//		if (!_resource._wholeMap)
//		{
//			Vector2I location = parentTower._gridLocation;
//			foreach (Vector2I relativePos in _resource._area)
//			{
//				Vector2I position = relativePos + location;
//				if (!parentTower._grid.IsCellVacant(position))
//				{
//					TowerUnit tower = parentTower._grid.GetTowerOnCell(position);
//					if (_resource._towers.Contains(tower._internalName))
//					{
//						_matches++;
//					}
//				}
//			}
//		}
//		else
//		{
//			foreach (TowerUnit tower in parentTower._tdManager._towerManager.GetAllTowers())
//			{
//				if (_resource._towers.Contains(tower._internalName))
//				{
//					_matches++;
//				}
//			}
//		}
//		if (_resource._cap != -1)
//		{
//			_matches = Math.Min(_matches, _resource._cap);
//		}
//		if (_matches > 0)
//		{
//			EffectResource resource = _resource._buffResource.MultiplyEffect(_matches);
//			_createdEffect = (InvaderStatsIncrease)_parentUnit.AddEffect(resource);
//		}
//	}

//	protected override void OnPlacedTower(TowerUnit tower)
//	{
//		TowerUnit parentTower = (TowerUnit)_parentUnit;
//		if (!_resource._wholeMap)
//		{
//			Vector2I delta = tower._gridLocation - parentTower._gridLocation;
//			if (_resource._area.Contains(delta) && _resource._towers.Contains(tower._internalName))
//			{
//				_matches++;
//				if (_resource._cap != -1)
//				{
//					_matches = Math.Min(_matches, _resource._cap);
//				}
//				InvaderStatsIncreaseResource resource = _resource._buffResource.MultiplyEffect(_matches);
//				if (_createdEffect != null)
//				{
//					_createdEffect.UpdateResource(resource);
//				}
//				else
//				{
//					_createdEffect = (InvaderStatsIncrease)_parentUnit.AddEffect(resource);
//				}
//			}
//		}
//		else
//		{
//			if (_resource._towers.Contains(tower._internalName))
//			{
//				_matches++;
//				if (_resource._cap != -1)
//				{
//					_matches = Math.Min(_matches, _resource._cap);
//				}
//				InvaderStatsIncreaseResource resource = _resource._buffResource.MultiplyEffect(_matches);
//				if (_createdEffect != null)
//				{
//					_createdEffect.UpdateResource(resource);
//				}
//				else
//				{
//					_createdEffect = (InvaderStatsIncrease)_parentUnit.AddEffect(resource);
//				}
//			}
//		}
//	}
//}
