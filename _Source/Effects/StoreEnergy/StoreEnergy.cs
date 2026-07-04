using Godot;
using RTSGame.Units;
using System;

public partial class StoreEnergy : Effect
{
	private StoreEnergyResource _resource;

	private bool _attackedThisTurn = false;

	public StoreEnergy(StoreEnergyResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		_parentUnit = unit;
		unit.Connect(Unit.SignalName.BeginAttack, Callable.From<Unit>(OnBeginAttack));
		unit.Connect(Unit.SignalName.NewWave, Callable.From(OnNewWave));
	}

	protected override void OnBeginAttack(Unit target)
	{
		if (!_attackedThisTurn)
		{
			_attackedThisTurn = true;
		}
	}

	protected override void OnNewWave()
	{
		if (!_attackedThisTurn)
		{
			_resource._turnsWithoutAttacking++;
			_resource.SetDescription();
			if (_resource._turnsWithoutAttacking < _resource._increaseLimit)
			{
				_parentUnit.IncreaseWeaponModifier(_resource._increaseAmount);
				_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
			}
			TowerUnit parentTower = (TowerUnit)_parentUnit;
			parentTower._tdManager.GainMoney(_resource._moneyGain);
			MakeFloatingAnimation("+" + Utils.MakeMoneyText(_resource._moneyGain));
		}
		else
		{
			_parentUnit.IncreaseWeaponModifier(-_resource._increaseAmount * (Math.Min((_resource._turnsWithoutAttacking), _resource._increaseLimit)));
			_resource._turnsWithoutAttacking = 0;
			_resource.SetDescription();
			_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		}
			_attackedThisTurn = false;
	}
}