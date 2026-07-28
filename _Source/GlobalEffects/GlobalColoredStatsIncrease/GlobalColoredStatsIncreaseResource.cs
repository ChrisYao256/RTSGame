using Godot;
using Godot.Collections;
using RTSGame._Source.Units;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalColoredStatsIncreaseResource : GlobalEffectResource
{
	[Export]
	public StatsIncreaseResource _statsIncrease;

	[Export]
	public int _color;

	public override void SetDescription()
	{
		string yellowHex = ThemePalette.Yellow.ToHtml(false);
		string redHex = ThemePalette.Red.ToHtml(false);
		string blueHex = ThemePalette.Blue.ToHtml(false);
		string greenHex = ThemePalette.Green.ToHtml(false);
		if (_color == 0)
		{
			_effectDescription = $"All [color=#{yellowHex}]yellow[/color] towers gain: \n";
		}
		else if (_color == 1)
		{
			_effectDescription = $"All [color=#{redHex}]red[/color] towers gain: \n";
		}
		else if (_color == 2)
		{
			_effectDescription = $"All [color=#{blueHex}]blue[/color] towers gain: \n";
		}
		else if (_color == 3)
		{
			_effectDescription = $"All [color=#{greenHex}]green[/color] towers gain: \n";
		}
		_statsIncrease.SetDescription();
		_effectDescription += _statsIncrease._effectDescription;
	}

	public override GlobalEffect CreateNode()
	{
		return new GlobalColoredStatsIncrease(this);
	}
}
