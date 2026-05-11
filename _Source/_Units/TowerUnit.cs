using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class TowerUnit : StationaryUnit
{
	[Export]
	public int _cost;

	[Export]
	public Texture2D _iconTexture;

	[Export]
	public string _description;

	[Export]
	public EffectResource _firstUpgrade;

	[Export]
	public int _firstUpgradeCost;

	[Export]
	public EffectResource _secondUpgradeA;
	[Export]
	public int _secondUpgradeACost;
	[Export]
	public EffectResource _secondUpgradeB;
	[Export]
	public int _secondUpgradeBCost;
	[Export]
	public EffectResource _secondUpgradeC;
	[Export]
	public int _secondUpgradeCCost;

	public bool _hasFirstUpgrade = false;

	public List<bool> _hasSecondUpgrade = [false, false, false];

	public override void _Ready()
	{
		base._Ready();
		CollisionLayer = UnitManager.TowerLayerMask;
		_aiControlled = false;
	}

	public void UpgradeFirst()
	{
		_hasFirstUpgrade = true;
		AddEffect(_firstUpgrade);
	}

	public void UpgradeSecondA()
	{
		_hasSecondUpgrade[0] = true;
		_secondUpgradeBCost *= 2;
		_secondUpgradeCCost *= 2;
		AddEffect( _secondUpgradeA);

	}

	public void UpgradeSecondB()
	{
		_hasSecondUpgrade[1] = true;
		_secondUpgradeCCost *= 2;
		_secondUpgradeACost *= 2;
		AddEffect(_secondUpgradeB);

	}

	public void UpgradeSecondC()
	{
		_hasSecondUpgrade[2] = true;
		_secondUpgradeBCost *= 2;
		_secondUpgradeACost *= 2;
		AddEffect(_secondUpgradeC);

	}

	public string GetDescription()
	{
		return _description;
	}

	public string GetDPS()
	{
		SetWeapon();
		if (_weapon is not null)
		{
			return _weapon.GetDPS().ToString();
		}
		else
		{
			return "No Weapon"; 
		}
	}
}

