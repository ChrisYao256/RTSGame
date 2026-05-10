using Godot;
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

	public override void _Ready()
	{
		base._Ready();
		CollisionLayer = UnitManager.TowerLayerMask;
		_aiControlled = false;
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

