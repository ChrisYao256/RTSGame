using Godot;
using Godot.Collections;
using RTSGame.Source;
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

	private float _upgradeCostScaling = 1.5f;

	public bool _hasFirstUpgrade = false;

	public List<bool> _hasSecondUpgrade = [false, false, false];

	public Grid _grid;
	public Vector2I _gridLocation;

	public override void _Ready()
	{
		_radius = TDManager.TileSize / 2f;
		_grid = GetTree().CurrentScene.GetNode<Grid>("TileMapLayer");
		base._Ready();
		CollisionLayer = UnitManager.TowerLayerMask;
		_aiControlled = false;
	}

	public override void SetSize()
	{
		base.SetSize();
		if (HasNode("TurretTurner"))
		{
			TurretTurner turret = GetNode<TurretTurner>("TurretTurner"); Sprite2D sprite = turret.GetNode<Sprite2D>("Sprite2D");
			Utils.ScaleVisualToRadius(sprite, _radius);
		}
	}

	public void UpgradeFirst()
	{
		_hasFirstUpgrade = true;
		AddEffect(_firstUpgrade);
	}

	public void UpgradeSecondA()
	{
		_hasSecondUpgrade[0] = true;
		_secondUpgradeBCost = (int)(_secondUpgradeBCost * _upgradeCostScaling);
		_secondUpgradeCCost = (int)(_secondUpgradeCCost * _upgradeCostScaling);
		AddEffect( _secondUpgradeA);

	}

	public void UpgradeSecondB()
	{
		_hasSecondUpgrade[1] = true;
		_secondUpgradeCCost = (int)(_secondUpgradeCCost * _upgradeCostScaling);
		_secondUpgradeACost = (int)(_secondUpgradeACost * _upgradeCostScaling);
		AddEffect(_secondUpgradeB);

	}

	public void UpgradeSecondC()
	{
		_hasSecondUpgrade[2] = true;
		_secondUpgradeBCost = (int)(_secondUpgradeBCost * _upgradeCostScaling);
		_secondUpgradeACost = (int)(_secondUpgradeACost * _upgradeCostScaling);
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

