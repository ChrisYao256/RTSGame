using Godot;
using Godot.Collections;
using System.Data;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalColoredSpawnerUpgradeResource : GlobalEffectResource
{
	[Export]
	public InvaderStatsIncreaseResource _statsIncrease;

	[Export]
	public int _color;

	public override GlobalEffect CreateNode()
	{
		return new GlobalColoredSpawnerUpgrade(this);
	}

	public override void SetDescription()
	{
		string yellowHex = ThemePalette.Yellow.ToHtml(false);
		string redHex = ThemePalette.Red.ToHtml(false);
		string blueHex = ThemePalette.Blue.ToHtml(false);
		string greenHex = ThemePalette.Green.ToHtml(false);
		if (_color == 0)
		{
			_effectDescription = $"All [color=#{yellowHex}]yellow[/color] portals gain: \n";
		}
		else if (_color == 1)
		{
			_effectDescription = $"All [color=#{redHex}]red[/color] portals gain: \n";
		}
		else if (_color == 2)
		{
			_effectDescription = $"All [color=#{blueHex}]blue[/color] portals gain: \n";
		}
		else if (_color == 3)
		{
			_effectDescription = $"All [color=#{greenHex}]green[/color] portals gain: \n";
		}
		_statsIncrease.SetDescription();
		_effectDescription += _statsIncrease._effectDescription;
	}
}
