using Godot;
using Godot.Collections;
using RTSGame._Source.Units;
using System.Collections.Generic;
using static BaseWeapon;
namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalWeaponTypeStatsIncreaseResource : GlobalEffectResource
{
	[Export]
	public StatsIncreaseResource _statsIncrease;

	[Export]
	public BaseWeapon.WeaponType _weaponType;

	public override void SetDescription()
	{
		string yellowHex = ThemePalette.Yellow.ToHtml(false);
		string redHex = ThemePalette.Red.ToHtml(false);
		string blueHex = ThemePalette.Blue.ToHtml(false);
		string greenHex = ThemePalette.Green.ToHtml(false);
		switch (_weaponType)
		{
			case (WeaponType.Scanner):
				_effectDescription = $"All ::scanner:: turrets gain: \n";
				break;
			case (WeaponType.Flame):
				_effectDescription = $"All ::flame:: turrets gain: \n";
				break;
			case (WeaponType.Projectile):
				_effectDescription = $"All ::projectile:: turrets gain: \n";
				break;
			case (WeaponType.Laser):
				_effectDescription = $"All ::laser:: turrets gain: \n";
				break;
			case (WeaponType.Ballistic):
				_effectDescription = $"All ::ballistic:: turrets gain: \n";
				break;
			case (WeaponType.Electric):
				_effectDescription = $"All ::electric:: turrets gain: \n";
				break;
		}
		_statsIncrease.SetDescription();
		_effectDescription += _statsIncrease._effectDescription;
	}

	public override GlobalEffect CreateNode()
	{
		return new GlobalWeaponTypeStatsIncrease(this);
	}
}
