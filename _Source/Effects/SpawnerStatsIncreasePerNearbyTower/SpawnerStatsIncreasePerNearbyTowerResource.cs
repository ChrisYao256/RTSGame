using Godot;
using Godot.Collections;
using System.Xml.Linq;

namespace RTSGame.Units;

[GlobalClass]
public partial class SpawnerStatsIncreasePerNearbyTowerResource : EffectResource
{
	[Export]
	public SpawnerStatsIncreaseResource _buffResource;

	[Export]
	public Array<Vector2I> _area = new Array<Vector2I>();

	[Export]
	public bool _wholeMap = false;

	[Export]
	public int _cap = -1;

	[Export]
	public Array<string> _towers = new Array<string>();

	public override void SetDescription()
	{
		_effectDescription = $"For {(_wholeMap ? "every" : "every nearby")} ";
		for (int i = 0; i < _towers.Count - 1; i++)
		{
			string name = UnitManager.InternalNameToName(_towers[i]);
			_effectDescription += name + " or ";
		}
		_effectDescription += UnitManager.InternalNameToName(_towers[_towers.Count - 1]) + ": \n";

		_buffResource.SetDescription();
		_effectDescription += _buffResource._effectDescription;
		if (_cap != -1)
		{
			_effectDescription += "Max stack: " + _cap;
		}
	}

	public override Effect CreateNode()
	{
		return new SpawnerStatsIncreasePerNearbyTower(this);
	}
}
