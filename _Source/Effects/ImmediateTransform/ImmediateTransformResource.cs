using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class ImmediateTransformResource : EffectResource
{
	[Export]
	public string _transformedUnit;

	public override void SetDescription()
	{
		Unit unit = UnitManager.GetUnit(_transformedUnit, false);
		if (unit is TowerUnit tower)
		{
			_effectDescription = tower._description;
		}
		unit.QueueFree();
	}

	public override Effect CreateNode()
	{
		return new ImmediateTransform(this);
	}
}
