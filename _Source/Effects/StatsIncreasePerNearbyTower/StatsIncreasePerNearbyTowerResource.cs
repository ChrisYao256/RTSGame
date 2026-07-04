//using Godot;
//using Godot.Collections;
//using System.Xml.Linq;

//namespace RTSGame.Units;

//[GlobalClass]
//public partial class StatsIncreasePerNearbyTowerResource : EffectResource
//{
//	[Export]
//	public StatsIncreaseResource _buffResource;

//	[Export]
//	public Array<Vector2I> _area = new Array<Vector2I>();

//	[Export]
//	public bool _wholeMap = false;

//	[Export]
//	public Array<string> _towers = new Array<string>();

//	public override void SetDescription()
//	{
//		_effectDescription = $"For {(_wholeMap ? "every" : "every nearby")} ";
//		for (int i = 0; i < _towers.Count - 1; i++)
//		{
//			string name = UnitManager.InternalNameToName(_towers[i]);
//			_effectDescription += name  + " or ";
//		}
//		_effectDescription += UnitManager.InternalNameToName(_towers[_towers.Count - 1]) + ": \n";
//		_buffResource.SetDescription();
//		_effectDescription += _buffResource._effectDescription;
//	}

//	public override Effect CreateNode()
//	{
//		return new StatsIncreasePerNearbyTower(this);
//	}
//}
